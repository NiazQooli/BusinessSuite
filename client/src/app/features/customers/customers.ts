import {
  Component,
  ElementRef,
  HostListener,
  OnDestroy,
  OnInit,
  ViewChild,
  inject,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import {
  Subject,
  Subscription,
  catchError,
  debounceTime,
  distinctUntilChanged,
  finalize,
  of,
  switchMap,
} from 'rxjs';
import { SalCustomerService } from '../../core/services/sal-customer.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { ToastService } from '../../core/services/toast.service';
import {
  SalCustomer,
  SalCustomerListItem,
  SaveSalCustomerRequest,
} from '../../core/models/sal-customer.model';

type FormMode = 'view' | 'add' | 'edit';

@Component({
  selector: 'app-customers',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './customers.html',
  styleUrl: './customers.scss',
})
export class CustomersComponent implements OnInit, OnDestroy {
  private readonly api = inject(SalCustomerService);
  private readonly confirm = inject(ConfirmDialogService);
  private readonly toast = inject(ToastService);

  @ViewChild('customerNameInput') customerNameInput?: ElementRef<HTMLInputElement>;

  readonly rows = signal<SalCustomerListItem[]>([]);
  readonly totalCount = signal(0);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly mode = signal<FormMode>('view');
  readonly selectedId = signal<string | null>(null);

  searchText = '';
  form: SalCustomer = this.emptyForm();

  /** Snapshot for Undo in Edit mode (VB Cancel / Undo). */
  private editSnapshot: SalCustomer | null = null;
  /** Last opened record — restore after New → Undo. */
  private lastViewedId: string | null = null;

  private readonly searchInput$ = new Subject<string>();
  private searchSub?: Subscription;

  ngOnInit(): void {
    this.searchSub = this.searchInput$
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        switchMap((term) => {
          this.loading.set(true);
          return this.api.search(1, 500, term).pipe(
            catchError((err: HttpErrorResponse) => {
              this.toast.error(this.readError(err, 'Failed to load customers.'));
              return of(null);
            }),
            finalize(() => this.loading.set(false)),
          );
        }),
      )
      .subscribe((result) => {
        if (!result) {
          return;
        }
        this.rows.set(result.items);
        this.totalCount.set(result.totalCount);
      });

    // Initial full list
    this.searchInput$.next('');
  }

  ngOnDestroy(): void {
    this.searchSub?.unsubscribe();
  }

  get titleMode(): string {
    switch (this.mode()) {
      case 'add':
        return 'Add';
      case 'edit':
        return 'Edit';
      default:
        return 'View';
    }
  }

  get isEditable(): boolean {
    return this.mode() === 'add' || this.mode() === 'edit';
  }

  /** Live find: type to filter; clear text to reload all. */
  onSearchInput(value: string): void {
    this.searchText = value;
    this.searchInput$.next(value.trim());
  }

  /** Find button / Enter — search immediately (no debounce wait). */
  onSearch(): void {
    this.searchInput$.next(this.searchText.trim());
  }

  private loadList(): void {
    this.searchInput$.next(this.searchText.trim());
  }

  selectRow(row: SalCustomerListItem): void {
    if (this.isEditable) {
      return;
    }
    this.openCustomer(row.customerId);
  }

  startNew(): void {
    this.mode.set('add');
    this.selectedId.set(null);
    this.editSnapshot = null;
    this.form = this.emptyForm();
    this.focusCustomerName();
  }

  startEdit(): void {
    if (!this.selectedId()) {
      this.toast.warning('Select a customer first.');
      return;
    }
    this.editSnapshot = { ...this.form };
    this.mode.set('edit');
    this.focusCustomerName();
  }

  async undo(): Promise<void> {
    if (!this.isEditable) {
      return;
    }

    const ok = await this.confirm.ask({
      title: 'Cancel changes',
      message: 'Discard current changes?',
      confirmText: 'Discard',
      cancelText: 'Keep editing',
      tone: 'default',
    });
    if (!ok) {
      return;
    }

    if (this.mode() === 'edit' && this.editSnapshot) {
      // Restore exact values from before Edit (do not depend on selectRow while editable).
      this.form = { ...this.editSnapshot };
      this.selectedId.set(this.editSnapshot.customerId);
      this.lastViewedId = this.editSnapshot.customerId;
      this.editSnapshot = null;
      this.mode.set('view');
      return;
    }

    // Add → Undo: return to last viewed record, or clear.
    this.editSnapshot = null;
    this.mode.set('view');
    const restoreId = this.lastViewedId;
    if (restoreId) {
      this.openCustomer(restoreId);
    } else {
      this.selectedId.set(null);
      this.form = this.emptyForm();
    }
  }

  async save(): Promise<void> {
    if (!this.form.customerCoName.trim()) {
      this.toast.warning('Customer Name cannot be empty.');
      this.focusCustomerName();
      return;
    }
    if (!this.form.address.trim()) {
      this.toast.warning('Address cannot be empty.');
      return;
    }

    const ok = await this.confirm.ask({
      title: this.mode() === 'add' ? 'Save new customer' : 'Save changes',
      message: 'Do you want to save this customer?',
      confirmText: 'Save',
      tone: 'default',
    });
    if (!ok) {
      return;
    }

    const payload = this.toRequest();
    this.saving.set(true);
    const wasAdd = this.mode() === 'add';

    const req$ = wasAdd
      ? this.api.create(payload)
      : this.api.update(this.selectedId()!, payload);

    req$.subscribe({
      next: (saved) => {
        this.saving.set(false);
        this.form = { ...saved };
        this.selectedId.set(saved.customerId);
        this.lastViewedId = saved.customerId;
        this.editSnapshot = null;
        this.mode.set('view');
        this.toast.success(`Customer ${saved.customerId} saved.`);
        this.loadList();
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(this.readError(err, 'Unable to save customer.'));
      },
    });
  }

  async delete(): Promise<void> {
    const id = this.selectedId();
    if (!id || this.isEditable) {
      return;
    }

    const ok = await this.confirm.ask({
      title: 'Delete customer',
      message: `Delete customer ${id}? This cannot be undone.`,
      confirmText: 'Delete',
      cancelText: 'Cancel',
      tone: 'danger',
    });
    if (!ok) {
      return;
    }

    this.saving.set(true);
    this.api.delete(id).subscribe({
      next: () => {
        this.saving.set(false);
        this.toast.success(`Customer ${id} deleted.`);
        this.selectedId.set(null);
        this.lastViewedId = null;
        this.editSnapshot = null;
        this.form = this.emptyForm();
        this.mode.set('view');
        this.loadList();
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(this.readError(err, 'Unable to delete customer.'));
      },
    });
  }

  /** VB Form_KeyPress: Enter moves to next field (not inside multiline Address). */
  @HostListener('keydown', ['$event'])
  onKeydown(event: KeyboardEvent): void {
    if (event.key !== 'Enter' || !this.isEditable) {
      return;
    }

    const target = event.target as HTMLElement | null;
    if (!target) {
      return;
    }

    if (target instanceof HTMLTextAreaElement) {
      return;
    }

    if (target.tagName !== 'INPUT') {
      return;
    }

    event.preventDefault();
    this.focusNextField(target as HTMLInputElement);
  }

  private openCustomer(customerId: string): void {
    this.loading.set(true);
    this.api.getById(customerId).subscribe({
      next: (customer) => {
        this.form = { ...customer };
        this.selectedId.set(customer.customerId);
        this.lastViewedId = customer.customerId;
        this.editSnapshot = null;
        this.mode.set('view');
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        this.toast.error(this.readError(err, 'Unable to open customer.'));
      },
    });
  }

  private focusCustomerName(): void {
    setTimeout(() => {
      const el = this.customerNameInput?.nativeElement;
      if (!el || el.disabled) {
        return;
      }
      el.focus();
      el.select();
    }, 0);
  }

  private focusNextField(current: HTMLInputElement): void {
    const root = current.closest('.cust__detail-panel');
    if (!root) {
      return;
    }

    const fields = Array.from(
      root.querySelectorAll<HTMLInputElement | HTMLTextAreaElement>(
        'input:not([type="checkbox"]):not([readonly]):not(:disabled), textarea:not(:disabled)',
      ),
    );

    const index = fields.indexOf(current);
    if (index < 0 || index >= fields.length - 1) {
      return;
    }

    const next = fields[index + 1];
    next.focus();
    if (next instanceof HTMLInputElement) {
      next.select();
    }
  }

  private toRequest(): SaveSalCustomerRequest {
    return {
      customerCoName: this.form.customerCoName,
      customerShortName: this.form.customerShortName,
      phone1: this.form.phone1,
      phone2: this.form.phone2,
      phone3: this.form.phone3,
      email: this.form.email,
      url: this.form.url,
      address: this.form.address,
      inactive: this.form.inactive,
    };
  }

  private emptyForm(): SalCustomer {
    return {
      customerId: '',
      customerCoName: '',
      customerShortName: '',
      phone1: '',
      phone2: '',
      phone3: '',
      email: '',
      url: '',
      address: '',
      inactive: false,
    };
  }

  private readError(err: HttpErrorResponse, fallback: string): string {
    const body = err.error as
      | { title?: string; errors?: Record<string, string[]>; detail?: string }
      | null;
    const first = body?.errors ? Object.values(body.errors).flat().find(Boolean) : undefined;
    return first || body?.detail || body?.title || fallback;
  }
}
