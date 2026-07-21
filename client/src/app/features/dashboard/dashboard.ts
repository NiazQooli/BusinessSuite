import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CustomerService } from '../../core/services/customer.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class DashboardComponent implements OnInit {
  private readonly customerService = inject(CustomerService);

  readonly customerCount = signal<number | null>(null);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.customerService.getCustomers(1, 1).subscribe({
      next: (result) => {
        this.customerCount.set(result.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Unable to reach the API. Start the .NET backend first.');
        this.loading.set(false);
      },
    });
  }
}
