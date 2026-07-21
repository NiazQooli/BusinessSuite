import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CustomerService } from '../../core/services/customer.service';
import { Customer } from '../../core/models/customer.model';

@Component({
  selector: 'app-customers',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './customers.html',
  styleUrl: './customers.scss',
})
export class CustomersComponent implements OnInit {
  private readonly customerService = inject(CustomerService);

  readonly customers = signal<Customer[]>([]);
  readonly totalCount = signal(0);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly search = signal('');

  name = '';
  email = '';
  phone = '';
  companyName = '';

  ngOnInit(): void {
    this.loadCustomers();
  }

  loadCustomers(): void {
    this.loading.set(true);
    this.error.set(null);

    this.customerService.getCustomers(1, 50, this.search()).subscribe({
      next: (result) => {
        this.customers.set(result.items);
        this.totalCount.set(result.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load customers. Is the API running?');
        this.loading.set(false);
      },
    });
  }

  onSearch(value: string): void {
    this.search.set(value);
    this.loadCustomers();
  }

  createCustomer(): void {
    if (!this.name.trim() || !this.email.trim()) {
      return;
    }

    this.customerService
      .createCustomer({
        name: this.name,
        email: this.email,
        phone: this.phone || null,
        companyName: this.companyName || null,
      })
      .subscribe({
        next: () => {
          this.name = '';
          this.email = '';
          this.phone = '';
          this.companyName = '';
          this.loadCustomers();
        },
        error: (err) => {
          this.error.set(err?.error?.title ?? 'Unable to create customer.');
        },
      });
  }

  deleteCustomer(id: string): void {
    this.customerService.deleteCustomer(id).subscribe({
      next: () => this.loadCustomers(),
      error: () => this.error.set('Unable to delete customer.'),
    });
  }
}
