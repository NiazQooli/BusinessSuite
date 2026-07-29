import { CommonModule } from '@angular/common';
import {
  ChangeDetectorRef,
  Component,
  ElementRef,
  HostListener,
  Injector,
  OnDestroy,
  OnInit,
  ViewChild,
  afterNextRender,
  inject,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { SalesInvoiceService } from '../../core/services/sales-invoice.service';
import {
  CustomerLookup,
  FinishedGoodLookup,
  SalesInvoiceLine,
  SalesInvoiceListItem,
  SalesInvoiceSearchParams,
} from '../../core/models/sales-invoice.model';
import { PrivilegeService } from '../../core/services/privilege.service';
import { UserPrivilege } from '../../core/models/privilege.model';
import { ToastService } from '../../core/services/toast.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { AuthService } from '../../core/services/auth.service';
import { writeInvoicePrintCache } from '../../core/services/invoice-print-cache';
import { FeIconComponent } from '../../shared/icon/fe-icon';

const PERIOD_STORAGE_KEY = 'fresher.accountingPeriod';

type FindSearchField = 'siType' | 'siNo' | 'siDate' | 'code' | 'customerName' | 'deliveredTo';
type FindEntryStatus = 'unchecked' | 'unposted' | 'posted';
type FindSortOn = 'authorized' | 'checked';

type FormMode = 'view' | 'add' | 'edit';

@Component({
  selector: 'app-sales-invoice',
  standalone: true,
  imports: [CommonModule, FormsModule, FeIconComponent],
  templateUrl: './sales-invoice.html',
  styleUrl: './sales-invoice.scss',
})
export class SalesInvoiceComponent implements OnInit, OnDestroy {
  private readonly api = inject(SalesInvoiceService);
  private readonly privilegesApi = inject(PrivilegeService);
  private readonly toast = inject(ToastService);
  private readonly confirmDialog = inject(ConfirmDialogService);
  private readonly auth = inject(AuthService);
  private readonly host = inject(ElementRef<HTMLElement>);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly injector = inject(Injector);

  fgDropdownStyle: Record<string, string> | null = null;
  customerDropdownStyle: Record<string, string> | null = null;
  private customerLookupAnchor: HTMLElement | null = null;
  private fgLookupAnchor: HTMLElement | null = null;
  private customerCache: CustomerLookup[] = [];
  private fgCache: FinishedGoodLookup[] = [];
  private customerSearchSub?: Subscription;
  private fgSearchSub?: Subscription;
  private prefetchSub?: Subscription;
  private findSearchSub?: Subscription;
  private findSearchDebounceSub?: Subscription;
  private readonly findSearchText$ = new Subject<string>();

  @ViewChild('salesEntryDateInput') salesEntryDateInput?: ElementRef<HTMLInputElement>;
  @ViewChild('deliveredToInput') deliveredToInput?: ElementRef<HTMLInputElement>;
  @ViewChild('customerCodeInput') customerCodeInput?: ElementRef<HTMLInputElement>;

  readonly mode = signal<FormMode>('view');
  readonly loading = signal(false);

  finYear = '';
  salesEntryType = 'BG';
  salesEntryId: number | null = null;
  salesEntryDate = '';
  vehicleRegNo = '';
  customerCode = '';
  customerName = '';
  address = '';
  distShop = '';
  deliveredTo = '';
  salesPromotion = false;
  checked = false;
  authorized = false;
  exportToPdf = false;

  totalWeight = 0;
  totalValue = 0;
  totalVat = 0;
  netValue = 0;

  lines: SalesInvoiceLine[] = [this.emptyLine()];

  // Customer inline lookup
  customerOptions: CustomerLookup[] = [];
  customerOpen = false;
  customerHighlight = 0;
  customerLoading = false;

  // FG inline lookup (per active row)
  fgOptions: FinishedGoodLookup[] = [];
  fgOpen = false;
  fgHighlight = 0;
  fgRowIndex = -1;
  fgLoading = false;

  private suppressEnterAdvance = false;

  // Find panel (VB frmSftSearch — Sales Invoice)
  findOpen = false;
  findSearch = '';
  findSearchField: FindSearchField = 'siNo';
  findEntryStatus: FindEntryStatus = 'unposted';
  findResults: SalesInvoiceListItem[] = [];
  findHighlight = 0;
  findLoading = false;
  userCanPost = false;
  userCanCheck = false;

  readonly findSearchFields: { id: FindSearchField; label: string }[] = [
    { id: 'siType', label: 'S.I TYPE' },
    { id: 'siNo', label: 'S.I NO.' },
    { id: 'siDate', label: 'S.I DATE' },
    { id: 'code', label: 'CODE' },
    { id: 'customerName', label: 'CUSTOMER NAME' },
    { id: 'deliveredTo', label: 'DELIVERED TO' },
  ];

  readonly findEntryStatusOptions: { value: FindEntryStatus; label: string }[] = [
    { value: 'unchecked', label: 'UnChecked' },
    { value: 'unposted', label: 'UnPosted' },
    { value: 'posted', label: 'Posted' },
  ];

  ngOnInit(): void {
    this.loading.set(true);
    this.api.getContext().subscribe({
      next: (ctx) => {
        // Prefer shell accounting-period FinYear (VB-style). Context alone picks
        // latest unlocked year (e.g. 2027-28) and hides older FY data.
        this.finYear = this.readStoredFinYear() || ctx.finYear;
        this.salesEntryDate = this.toInputDate(new Date());
        this.loading.set(false);
      },
      error: (err) => {
        if (err?.status !== 401) {
          this.toast.error('Unable to load sales invoice context. Is the API running?');
        }
        this.finYear = this.readStoredFinYear() || this.finYear;
        this.loading.set(false);
      },
    });

    this.privilegesApi.getMine().subscribe({
      next: (rows) => this.applyUserPrivileges(rows),
      error: () => {
        this.userCanPost = false;
        this.userCanCheck = false;
      },
    });

    this.findSearchDebounceSub = this.findSearchText$
      .pipe(debounceTime(220), distinctUntilChanged())
      .subscribe((term) => {
        this.findSearch = term;
        this.loadFind();
      });
  }

  ngOnDestroy(): void {
    this.customerSearchSub?.unsubscribe();
    this.fgSearchSub?.unsubscribe();
    this.prefetchSub?.unsubscribe();
    this.findSearchSub?.unsubscribe();
    this.findSearchDebounceSub?.unsubscribe();
    this.findSearchText$.complete();
  }

  get findSortOn(): FindSortOn {
    return this.findEntryStatus === 'unchecked' ? 'checked' : 'authorized';
  }

  get findSortFlag(): boolean {
    return this.findEntryStatus === 'posted';
  }

  get isEditable(): boolean {
    return this.mode() === 'add' || this.mode() === 'edit';
  }

  get titleMode(): string {
    return this.mode() === 'add' ? 'Add' : this.mode() === 'edit' ? 'Edit' : 'View';
  }

  onNew(): void {
    this.clearForm();
    this.mode.set('add');
    this.salesEntryDate = this.toInputDate(new Date());
    this.lines = [this.emptyLine()];
    this.prefetchLookups();
    afterNextRender(() => this.focusFirstEditableField(), { injector: this.injector });
  }

  onEdit(): void {
    if (!this.salesEntryId || this.authorized) {
      return;
    }
    this.mode.set('edit');
    this.prefetchLookups();
    afterNextRender(() => this.focusFirstEditableField(), { injector: this.injector });
  }

  async onCancel(): Promise<void> {
    const ok = await this.confirmDialog.ask({
      title: 'Cancel changes',
      message: 'Cancel current changes?',
      confirmText: 'Yes, cancel',
      cancelText: 'Keep editing',
      tone: 'warning',
    });
    if (!ok) {
      return;
    }
    this.mode.set('view');
    this.clearForm();
  }

  onClose(): void {
    if (this.isEditable) {
      void this.onCancel();
      return;
    }
    this.clearForm();
    this.mode.set('view');
  }

  async onDelete(): Promise<void> {
    if (!this.salesEntryId || !this.finYear || this.authorized) {
      return;
    }
    const ok = await this.confirmDialog.ask({
      title: 'Delete invoice',
      message: 'Delete this sales invoice?',
      confirmText: 'Delete',
      cancelText: 'Cancel',
      tone: 'danger',
    });
    if (!ok) {
      return;
    }

    const id = this.salesEntryId;
    const finYear = this.finYear;
    this.loading.set(true);
    this.api.delete(id, finYear).subscribe({
      next: () => {
        this.resetAfterDelete();
        this.toast.success('Sales invoice deleted.');
      },
      error: (err) => {
        this.loading.set(false);
        this.cdr.detectChanges();
        this.toast.error(err?.error?.detail ?? 'Delete failed.');
      },
    });
  }

  private resetAfterDelete(): void {
    this.clearForm();
    this.mode.set('view');
    this.salesEntryDate = this.toInputDate(new Date());
    this.exportToPdf = false;
    this.customerOpen = false;
    this.fgOpen = false;
    this.customerOptions = [];
    this.fgOptions = [];
    this.customerDropdownStyle = null;
    this.fgDropdownStyle = null;
    this.loading.set(false);
    this.cdr.detectChanges();
  }

  async onSave(): Promise<void> {
    if (!this.validate()) {
      return;
    }
    const ok = await this.confirmDialog.ask({
      title: 'Save invoice',
      message: 'Save sales invoice?',
      confirmText: 'Save',
      cancelText: 'Cancel',
      tone: 'success',
    });
    if (!ok) {
      return;
    }

    this.recalculateTotals();
    const payload = {
      finYear: this.finYear,
      salesEntryDate: this.salesEntryDate,
      vehicleRegNo: this.vehicleRegNo,
      customerId: this.customerCode.trim(),
      deliveredTo: this.deliveredTo,
      totalExcStValue: this.totalValue,
      totalVatValue: this.totalVat,
      netAmount: this.netValue,
      totalWeightInTons: this.totalWeight,
      lines: this.lines
        .filter((l) => l.fgId && (l.noOfPacks ?? 0) > 0)
        .map((l) => ({
          fgId: l.fgId!,
          noOfPacks: l.noOfPacks ?? 0,
          weight: l.weight ?? 0,
          basicRate: l.basicRate ?? 0,
          specialDiscount: l.specialDiscount,
          excStRate: l.excStRate,
          incStValue: l.incStValue,
          stRate: l.stRate,
          stAmount: l.stAmount,
          excStValue: l.excStValue,
          netRate: l.netRate,
          emptyCartons: l.emptyCartons,
          freightCharges: l.freightCharges,
          soapCharges: l.soapCharges,
          distributorMargin: l.distributorMargin,
          retailerMargin: l.retailerMargin,
          leakageRate: l.leakageRate,
          vatPercentage: l.vatPercentage ?? 0,
          vatValue: l.vatValue,
          discount1Percentage: l.discount1Percentage,
          discount1PercentageValue: l.discount1PercentageValue,
          discount2PerPack: l.discount2PerPack,
          discount2PerPackValue: l.discount2PerPackValue,
          stAmount17: l.stAmount17,
          stAmount03: l.stAmount03,
        })),
    };

    const req$ =
      this.mode() === 'edit' && this.salesEntryId
        ? this.api.update(this.salesEntryId, payload)
        : this.api.create(payload);

    this.loading.set(true);
    req$.subscribe({
      next: (inv) => {
        this.applyInvoice(inv);
        this.mode.set('view');
        this.loading.set(false);
        this.toast.success('Sales invoice saved.');
      },
      error: (err) => {
        this.loading.set(false);
        this.toast.error(err?.error?.detail ?? err?.error?.title ?? 'Save failed.');
      },
    });
  }

  async onCheck(): Promise<void> {
    if (!this.salesEntryId || !this.finYear) {
      return;
    }
    const ok = await this.confirmDialog.ask({
      title: 'Check invoice',
      message: 'Check this sales invoice?',
      confirmText: 'Check',
      cancelText: 'Cancel',
      tone: 'default',
    });
    if (!ok) {
      return;
    }
    this.api.check(this.salesEntryId, this.finYear).subscribe({
      next: () => {
        this.clearForm();
        this.mode.set('view');
        this.toast.success('Sales invoice checked.');
      },
      error: (err) => this.toast.error(err?.error?.detail ?? 'Check failed.'),
    });
  }

  async onPost(): Promise<void> {
    if (!this.salesEntryId || !this.finYear) {
      return;
    }
    const ok = await this.confirmDialog.ask({
      title: 'Post invoice',
      message: 'Post this sales invoice?',
      confirmText: 'Post',
      cancelText: 'Cancel',
      tone: 'warning',
    });
    if (!ok) {
      return;
    }
    this.api.post(this.salesEntryId, this.finYear).subscribe({
      next: () => {
        this.authorized = true;
        this.toast.success('Sales invoice posted.');
      },
      error: (err) => this.toast.error(err?.error?.detail ?? 'Post failed.'),
    });
  }

  /** VB enztb_ClickPreview → opens A4 invoice in a new browser tab (CUSTOMER COPY). */
  onPreview(): void {
    if (!this.salesEntryId || this.isEditable) {
      this.toast.error('Save / load a sales invoice before preview.');
      return;
    }
    const lines = this.lines.filter((l) => l.fgId && (l.noOfPacks ?? 0) > 0);
    if (lines.length === 0) {
      this.toast.error('Commercial Invoice — CUSTOMER COPY cannot be printed.');
      return;
    }

    const session = this.auth.getSession();
    const printedBy = (
      session?.fullName ||
      session?.userName ||
      session?.userId ||
      'USER'
    ).toUpperCase();

    writeInvoicePrintCache({
      salesEntryId: this.salesEntryId,
      finYear: this.finYear,
      printedBy,
      cachedAt: new Date().toISOString(),
      invoice: {
        finYear: this.finYear,
        billType: 'SI',
        salesEntryType: this.salesEntryType || 'BG',
        salesEntryId: this.salesEntryId,
        salesEntryDate: this.salesEntryDate,
        vehicleRegNo: this.vehicleRegNo,
        customerId: this.customerCode || null,
        customerName: this.customerName || null,
        address: this.address || null,
        distShop: this.distShop || null,
        salesPromotion: this.salesPromotion,
        deliveredTo: this.deliveredTo || 'N/A',
        stockLocationId: null,
        totalExcStValue: this.totalValue,
        totalVatValue: this.totalVat,
        netAmount: this.netValue,
        totalWeightInTons: this.totalWeight,
        checked: this.checked,
        authorized: this.authorized,
        lines: lines.map((l) => ({ ...l })),
      },
    });

    const finYear = encodeURIComponent(this.finYear);
    const autoPrint = this.exportToPdf ? '1' : '0';
    const url = `/print/invoice/${this.salesEntryId}?finYear=${finYear}&autoPrint=${autoPrint}`;
    const win = window.open(url, '_blank');
    if (!win) {
      this.toast.error('Pop-up blocked. Allow pop-ups for invoice preview.');
      return;
    }
    win.focus();
  }

  openFind(): void {
    this.syncFinYearFromShell();
    this.findOpen = true;
    this.findSearch = '';
    this.findSearchField = 'siNo';
    this.findEntryStatus = this.userCanPost ? 'unposted' : 'unchecked';
    this.findHighlight = 0;
    this.findResults = [];
    this.loadFind();
  }

  onFindSearchText(term: string): void {
    this.findSearchText$.next(term ?? '');
  }

  onFindFilterChange(): void {
    this.loadFind();
  }

  loadFind(): void {
    this.syncFinYearFromShell();
    if (!this.finYear) {
      return;
    }
    const periodMeta = this.readAccountingPeriodMeta();
    // Full financial year: FinYear filter is enough (VB). Date range only for sub-periods.
    const applyDates = !!periodMeta?.start && !!periodMeta?.end && !periodMeta.isFullYear;
    const params: SalesInvoiceSearchParams = {
      finYear: this.finYear,
      search: this.findSearch,
      searchField: this.findSearchField,
      sortOn: this.findSortOn,
      sortFlag: this.findSortFlag,
      periodStart: applyDates ? periodMeta!.start : undefined,
      periodEnd: applyDates ? periodMeta!.end : undefined,
      take: 500,
    };

    this.findLoading = true;
    this.cdr.detectChanges();

    this.findSearchSub?.unsubscribe();
    this.findSearchSub = this.api.search(params).subscribe({
      next: (rows) => {
        this.findResults = rows;
        this.findHighlight = rows.length ? 0 : -1;
        this.findLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.findResults = [];
        this.findHighlight = -1;
        this.findLoading = false;
        this.cdr.detectChanges();
        this.toast.error('Unable to load find list.');
      },
    });
  }

  closeFind(): void {
    this.findSearchSub?.unsubscribe();
    this.findOpen = false;
    this.findLoading = false;
  }

  confirmFind(): void {
    if (this.findHighlight < 0 || !this.findResults[this.findHighlight]) {
      return;
    }
    this.selectFind(this.findResults[this.findHighlight]);
  }

  selectFind(row: SalesInvoiceListItem): void {
    this.findOpen = false;
    this.loading.set(true);
    this.api.getById(row.salesEntryId, this.finYear).subscribe({
      next: (inv) => {
        this.clearForm();
        this.applyInvoice(inv);
        this.mode.set('view');
        this.exportToPdf = false;
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.toast.error('Unable to load invoice.');
      },
    });
  }

  formatFindNetAmount(amount: number): string {
    const n = Number(amount) || 0;
    const hasPence = Math.abs(n % 1) >= 0.005;
    return n.toLocaleString('en-GB', {
      minimumFractionDigits: hasPence ? 2 : 0,
      maximumFractionDigits: 2,
      useGrouping: true,
    });
  }

  /** Money / price display with thousand separators (en-GB). */
  formatMoney(value: number | string | null | undefined, fractionDigits = 2): string {
    if (value == null || value === '') {
      return '';
    }
    const n = Number(value);
    if (Number.isNaN(n)) {
      return '';
    }
    return n.toLocaleString('en-GB', {
      minimumFractionDigits: fractionDigits,
      maximumFractionDigits: fractionDigits,
      useGrouping: true,
    });
  }

  onFindKey(event: KeyboardEvent): void {
    if (!this.findOpen) {
      return;
    }
    if (event.key === 'Escape') {
      event.preventDefault();
      this.closeFind();
      return;
    }
    if (this.findResults.length === 0) {
      return;
    }
    if (event.key === 'ArrowDown') {
      event.preventDefault();
      this.findHighlight = (this.findHighlight + 1) % this.findResults.length;
    } else if (event.key === 'ArrowUp') {
      event.preventDefault();
      this.findHighlight =
        (this.findHighlight - 1 + this.findResults.length) % this.findResults.length;
    } else if (event.key === 'Enter' && this.findHighlight >= 0) {
      event.preventDefault();
      this.confirmFind();
    }
  }

  onSalesPromotionChange(): void {
    if (!this.isEditable) {
      return;
    }
    if (this.salesPromotion) {
      this.customerCode = '99001';
      this.customerName = 'SALES PROMOTION';
      this.address = '';
      this.distShop = '';
      setTimeout(() => this.deliveredToInput?.nativeElement?.focus(), 0);
    } else {
      this.customerCode = '';
      this.customerName = '';
      this.address = '';
      this.distShop = '';
    }
  }

  onCustomerFocus(event: FocusEvent | MouseEvent): void {
    if (!this.isEditable || this.salesPromotion) {
      return;
    }
    const target = event.target;
    if (!(target instanceof HTMLElement)) {
      return;
    }
    this.customerLookupAnchor = target.tagName === 'INPUT' ? target : (this.customerCodeInput?.nativeElement ?? target);
    // Cursor in field â†’ open full LOV immediately (type to filter)
    this.openCustomerLookup('');
  }

  onCustomerInput(event: Event): void {
    if (!this.isEditable || this.salesPromotion) {
      return;
    }
    if (event.target instanceof HTMLElement) {
      this.customerLookupAnchor = event.target;
    }
    this.customerName = '';
    this.address = '';
    this.distShop = '';
    this.openCustomerLookup(this.customerCode);
  }

  openCustomerLookup(term: string): void {
    const q = (term ?? '').trim();
    this.customerOpen = true;
    this.customerLoading = true;

    // Instant list from cache so dropdown opens on focus without waiting for API
    const cached = this.filterCustomerCache(q);
    if (cached.length) {
      this.customerOptions = cached;
      this.customerHighlight = 0;
    }

    if (this.customerLookupAnchor) {
      this.customerDropdownStyle = this.buildDropdownStyle(this.customerLookupAnchor, 0);
    }

    this.customerSearchSub?.unsubscribe();
    this.customerSearchSub = this.api.searchCustomers(q, 100).subscribe({
      next: (rows) => {
        if (!q) {
          this.customerCache = rows;
        }
        this.customerOptions = rows;
        this.customerOpen = true;
        this.customerHighlight = rows.length ? 0 : -1;
        this.customerLoading = false;
        if (this.customerLookupAnchor) {
          this.customerDropdownStyle = this.buildDropdownStyle(this.customerLookupAnchor, 0);
        }
        this.cdr.detectChanges();
        this.scrollActiveLookupOption('customer');
      },
      error: () => {
        this.customerLoading = false;
        if (!this.customerOptions.length) {
          this.customerOpen = false;
          this.customerDropdownStyle = null;
        }
        this.cdr.detectChanges();
      },
    });
  }

  onCustomerKey(event: KeyboardEvent): void {
    if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
      event.preventDefault();
      event.stopPropagation();
      if (!this.customerOpen || this.customerOptions.length === 0) {
        this.customerLookupAnchor = event.target as HTMLElement;
        this.openCustomerLookup(this.customerCode);
        return;
      }
      if (event.key === 'ArrowDown') {
        this.customerHighlight = (this.customerHighlight + 1) % this.customerOptions.length;
      } else {
        this.customerHighlight =
          (this.customerHighlight - 1 + this.customerOptions.length) %
          this.customerOptions.length;
      }
      this.scrollActiveLookupOption('customer');
      return;
    }

    if (event.key === 'Enter' && this.customerOpen && this.customerOptions.length > 0) {
      event.preventDefault();
      event.stopPropagation();
      this.suppressEnterAdvance = true;
      this.selectCustomer(this.customerOptions[this.customerHighlight], true);
      return;
    }

    if (event.key === 'Escape') {
      event.preventDefault();
      this.customerOpen = false;
    }
  }

  selectCustomer(row: CustomerLookup, advance = false): void {
    this.customerCode = row.customerId;
    this.customerName = row.customerName;
    this.address = row.address ?? '';
    this.distShop = row.distShop ?? '';
    this.customerOpen = false;
    this.customerOptions = [];
    this.customerDropdownStyle = null;
    if (advance) {
      setTimeout(() => this.focusFieldByName('delivered'), 0);
    }
  }

  onFgFocus(rowIndex: number, event: FocusEvent | MouseEvent): void {
    if (!this.isEditable) {
      return;
    }
    this.fgRowIndex = rowIndex;
    const target = event.target;
    if (target instanceof HTMLElement) {
      this.fgLookupAnchor = target.tagName === 'INPUT' ? target : target;
    }
    // Cursor in field â†’ open full FG list immediately
    this.openFgLookup('');
  }

  onFgInput(rowIndex: number, event: Event): void {
    if (!this.isEditable) {
      return;
    }
    this.fgRowIndex = rowIndex;
    if (event.target instanceof HTMLElement) {
      this.fgLookupAnchor = event.target;
    }
    const line = this.lines[rowIndex];
    this.clearLineDetails(line);
    this.openFgLookup(line.fgId?.toString() ?? '');
  }

  openFgLookup(term: string): void {
    const q = (term ?? '').trim();
    this.fgOpen = true;
    this.fgLoading = true;

    const cached = this.filterFgCache(q);
    if (cached.length) {
      this.fgOptions = cached;
      this.fgHighlight = 0;
    }

    if (this.fgLookupAnchor) {
      this.fgDropdownStyle = this.buildDropdownStyle(this.fgLookupAnchor, 280);
    }

    this.fgSearchSub?.unsubscribe();
    this.fgSearchSub = this.api.searchFinishedGoods(q, 100).subscribe({
      next: (rows) => {
        if (!q) {
          this.fgCache = rows;
        }
        this.fgOptions = rows;
        this.fgOpen = true;
        this.fgHighlight = rows.length ? 0 : -1;
        this.fgLoading = false;
        if (this.fgLookupAnchor) {
          this.fgDropdownStyle = this.buildDropdownStyle(this.fgLookupAnchor, 280);
        }
        this.cdr.detectChanges();
        this.scrollActiveLookupOption('fg');
      },
      error: () => {
        this.fgLoading = false;
        if (!this.fgOptions.length) {
          this.fgOpen = false;
          this.fgDropdownStyle = null;
        }
        this.cdr.detectChanges();
      },
    });
  }

  onFgKey(event: KeyboardEvent, rowIndex: number): void {
    this.fgRowIndex = rowIndex;

    if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
      event.preventDefault();
      event.stopPropagation();
      if (!this.fgOpen || this.fgOptions.length === 0) {
        this.fgLookupAnchor = event.target as HTMLElement;
        this.openFgLookup(this.lines[rowIndex].fgId?.toString() ?? '');
        return;
      }
      if (event.key === 'ArrowDown') {
        this.fgHighlight = (this.fgHighlight + 1) % this.fgOptions.length;
      } else {
        this.fgHighlight =
          (this.fgHighlight - 1 + this.fgOptions.length) % this.fgOptions.length;
      }
      this.scrollActiveLookupOption('fg');
      return;
    }

    if (event.key === 'Enter' && this.fgOpen && this.fgOptions.length > 0) {
      event.preventDefault();
      event.stopPropagation();
      this.suppressEnterAdvance = true;
      this.selectFg(this.fgOptions[this.fgHighlight], rowIndex, true);
      return;
    }

    if (event.key === 'Escape') {
      event.preventDefault();
      this.fgOpen = false;
      this.fgDropdownStyle = null;
    }
  }

  selectFg(fg: FinishedGoodLookup, rowIndex: number, advance = false): void {
    if (this.lines.some((l, i) => i !== rowIndex && l.fgId === fg.fgId)) {
      this.toast.error('Finished Goods Code has already been entered.');
      return;
    }

    const line = this.lines[rowIndex];
    line.fgId = fg.fgId;
    line.brandName = fg.brandName;
    line.fgName = fg.fgName;
    line.uom = fg.uom;
    line.noOfUnitsInPack = fg.noOfUnitsInPack;
    line.unitWeight = fg.unitWeight;
    line.forTonsConversion = fg.forTonsConversion;
    line.emptyCartonPercentage = fg.emptyCartonPercentage;
    line.distributorMargin = fg.distributorMargin;
    line.retailerMargin = fg.retailerMargin;
    line.sgId = fg.sgId;
    line.stRate = 0;
    line.vatPercentage = fg.vatPercentage || 20;
    line.basicRate = fg.basicRate || null;
    line.specialDiscount = fg.specialDiscount;
    line.freightCharges = fg.freightCharges;
    line.soapCharges = fg.soapCharges;
    line.leakageRate = fg.leakageRate;
    this.fgOpen = false;
    this.fgDropdownStyle = null;
    this.fgOptions = [];
    this.recalcLine(rowIndex);
    if (advance) {
      setTimeout(() => this.focusGridField(rowIndex, 'packs'), 0);
    }
  }

  onLineChange(rowIndex: number): void {
    this.recalcLine(rowIndex);
  }

  /** VB Advance: add blank row when last row has FG + packs + rate. */
  addLine(focusNew = true): void {
    if (!this.isEditable) {
      return;
    }
    const last = this.lines[this.lines.length - 1];
    if (last && !last.fgId && !(last.noOfPacks ?? 0) && !(last.basicRate ?? 0)) {
      if (focusNew) {
        setTimeout(() => this.focusGridField(this.lines.length - 1, 'fg'), 0);
      }
      return;
    }
    this.lines = [...this.lines, this.emptyLine()];
    if (focusNew) {
      setTimeout(() => this.focusGridField(this.lines.length - 1, 'fg'), 0);
    }
  }

  private tryAdvanceNewRowLikeVb(rowIndex: number): boolean {
    const line = this.lines[rowIndex];
    if (!line) {
      return false;
    }
    if (!line.fgId || !(line.noOfPacks ?? 0) || !(line.basicRate ?? 0)) {
      return false;
    }
    if (rowIndex !== this.lines.length - 1) {
      return false;
    }
    this.lines = [...this.lines, this.emptyLine()];
    setTimeout(() => this.focusGridField(this.lines.length - 1, 'fg'), 0);
    return true;
  }

  removeLine(rowIndex: number): void {
    if (!this.isEditable) {
      return;
    }
    this.lines.splice(rowIndex, 1);
    if (this.lines.length === 0) {
      this.lines = [this.emptyLine()];
    }
    this.recalculateTotals();
  }

  @HostListener('document:keydown', ['$event'])
  onEnterAdvance(event: KeyboardEvent): void {
    if (event.key !== 'Enter' || event.isComposing) {
      return;
    }
    if (this.suppressEnterAdvance) {
      this.suppressEnterAdvance = false;
      return;
    }
    if (!this.isEditable || this.findOpen) {
      return;
    }

    const target = event.target as HTMLElement | null;
    if (!target || !this.host.nativeElement.contains(target)) {
      return;
    }
    if (target.tagName === 'BUTTON' || target.closest('.si-find')) {
      return;
    }
    if (this.customerLoading || this.fgLoading) {
      event.preventDefault();
      return;
    }
    if (this.customerOpen && this.customerOptions.length > 0) {
      return;
    }
    if (this.fgOpen && this.fgOptions.length > 0) {
      return;
    }

    const focusable = this.getFocusableFields();
    const current = target.closest('input, select, textarea') as HTMLElement | null;
    if (!current || !focusable.includes(current)) {
      return;
    }

    event.preventDefault();

    const field = current.getAttribute('data-si-field');
    const rowAttr = current.getAttribute('data-row');
    const rowIndex = rowAttr != null ? Number(rowAttr) : -1;

    // VB grid Advance: last editable cell of last complete row â†’ new row
    if (field === 'vat' && rowIndex >= 0 && this.tryAdvanceNewRowLikeVb(rowIndex)) {
      return;
    }

    const idx = focusable.indexOf(current);
    if (idx >= 0 && idx < focusable.length - 1) {
      this.focusElement(focusable[idx + 1]);
      return;
    }

    // Last focusable overall: if on last grid row and complete, add row
    if (rowIndex >= 0 && this.tryAdvanceNewRowLikeVb(rowIndex)) {
      return;
    }
  }

  @HostListener('document:mousedown', ['$event'])
  onDocPointerDown(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    if (target.closest('.si__lookup')) {
      return;
    }
    this.customerOpen = false;
    this.fgOpen = false;
    this.customerDropdownStyle = null;
    this.fgDropdownStyle = null;
  }

  private validate(): boolean {
    if (!this.salesEntryDate) {
      this.toast.error('Sales Invoice Date is not valid.');
      return false;
    }
    if (!this.customerCode.trim() || !this.customerName.trim()) {
      this.toast.error('Customer Code must be entered properly.');
      return false;
    }
    const validLines = this.lines.filter(
      (l) => l.fgId || (l.noOfPacks ?? 0) > 0 || (l.basicRate ?? 0) > 0,
    );
    if (validLines.length === 0) {
      this.toast.error('At least one transaction is required in detail section.');
      return false;
    }
    for (let i = 0; i < validLines.length; i++) {
      const l = validLines[i];
      if (!l.fgId || !l.fgName) {
        this.toast.error(`FG Code is not valid. Line No.: ${i + 1}.`);
        return false;
      }
      if (!(l.noOfPacks ?? 0)) {
        this.toast.error(`No. Of Packs cannot be zero(0). Line No.: ${i + 1}.`);
        return false;
      }
      if (!(l.basicRate ?? 0)) {
        this.toast.error(`Basic Rate cannot be zero(0). Line No.: ${i + 1}.`);
        return false;
      }
      const vat = l.vatPercentage ?? 0;
      if (vat > 0 && vat !== 20) {
        this.toast.error(`Standard VAT Rate is 20%. Please check input. Line No.: ${i + 1}.`);
        return false;
      }
    }
    return true;
  }

  private applyInvoice(inv: {
    salesEntryId: number;
    salesEntryDate: string;
    salesEntryType?: string;
    vehicleRegNo: string;
    customerId: string | null;
    customerName: string | null;
    address: string | null;
    distShop: string | null;
    salesPromotion: boolean;
    deliveredTo: string;
    totalExcStValue: number;
    totalVatValue: number;
    netAmount: number;
    totalWeightInTons: number;
    checked: boolean;
    authorized: boolean;
    lines: SalesInvoiceLine[];
  }): void {
    this.salesEntryId = inv.salesEntryId;
    this.salesEntryType = inv.salesEntryType?.trim() || 'BG';
    this.salesEntryDate = this.toInputDate(new Date(inv.salesEntryDate));
    this.vehicleRegNo = inv.vehicleRegNo ?? '';
    this.customerCode = inv.customerId ?? '';
    this.customerName = inv.customerName ?? '';
    this.address = inv.address ?? '';
    this.distShop = inv.distShop ?? '';
    this.salesPromotion = inv.salesPromotion;
    this.deliveredTo = inv.deliveredTo === 'N/A' ? '' : inv.deliveredTo;
    this.checked = inv.checked;
    this.authorized = inv.authorized;
    this.lines = inv.lines.length
      ? inv.lines.map((l) => ({ ...l }))
      : [this.emptyLine()];
    this.totalValue = inv.totalExcStValue;
    this.totalVat = inv.totalVatValue;
    this.netValue = inv.netAmount;
    this.totalWeight = inv.totalWeightInTons;
  }

  private recalcLine(rowIndex: number): void {
    const line = this.lines[rowIndex];
    const packs = Number(line.noOfPacks) || 0;
    const basic = Number(line.basicRate) || 0;
    const special = Number(line.specialDiscount) || 0;
    const vatPct = Number(line.vatPercentage) || 0;
    const stRate = Number(line.stRate) || 0;

    line.weight = round2(line.noOfUnitsInPack * line.unitWeight * packs);

    const excRate =
      stRate > 0
        ? round3(basic - round3(special / (1 + stRate / 100)))
        : round3(basic - special);
    line.excStRate = excRate;
    line.excStValue = round2(packs * excRate);

    const netRate = round3(
      excRate + round3(excRate * (vatPct / 100)) + round3(excRate * (stRate / 100)),
    );
    line.netRate = netRate;
    line.incStValue = round2(packs * netRate);

    const vatAmount =
      vatPct > 0 ? round2(round2(packs * round3(excRate + round3(excRate * (vatPct / 100)))) - line.excStValue) : 0;
    line.vatValue = vatAmount;

    line.emptyCartons = Math.round(packs * (line.emptyCartonPercentage / 100));
    line.discount1PercentageValue = round2(line.excStValue * (line.discount1Percentage / 100));
    line.discount2PerPackValue = round2(packs * line.discount2PerPack);

    this.recalculateTotals();
  }

  private recalculateTotals(): void {
    let tons = 0;
    let value = 0;
    let vat = 0;
    for (const line of this.lines) {
      tons += ((Number(line.weight) || 0) * (Number(line.forTonsConversion) || 0)) / 1000;
      value += Number(line.excStValue) || 0;
      vat += Number(line.vatValue) || 0;
    }
    this.totalWeight = round2(tons);
    this.totalValue = round2(value);
    this.totalVat = round2(vat);
    this.netValue = round2(value + vat);
  }

  private clearLineDetails(line: SalesInvoiceLine): void {
    line.brandName = '';
    line.fgName = '';
    line.uom = '';
    line.weight = null;
    line.basicRate = null;
    line.excStValue = 0;
    line.netRate = 0;
    line.vatValue = 0;
  }

  private clearForm(): void {
    this.salesEntryId = null;
    this.vehicleRegNo = '';
    this.customerCode = '';
    this.customerName = '';
    this.address = '';
    this.distShop = '';
    this.deliveredTo = '';
    this.salesPromotion = false;
    this.checked = false;
    this.authorized = false;
    this.exportToPdf = false;
    this.lines = [this.emptyLine()];
    this.totalWeight = 0;
    this.totalValue = 0;
    this.totalVat = 0;
    this.netValue = 0;
    this.customerOpen = false;
    this.fgOpen = false;
    this.customerOptions = [];
    this.fgOptions = [];
    this.customerDropdownStyle = null;
    this.fgDropdownStyle = null;
  }

  private emptyLine(): SalesInvoiceLine {
    return {
      fgId: null,
      brandName: '',
      fgName: '',
      noOfPacks: null,
      weight: null,
      uom: '',
      basicRate: null,
      specialDiscount: 0,
      excStRate: 0,
      incStValue: 0,
      stRate: 0,
      stAmount: 0,
      excStValue: 0,
      netRate: 0,
      emptyCartons: 0,
      noOfUnitsInPack: 0,
      unitWeight: 0,
      forTonsConversion: 0,
      emptyCartonPercentage: 0,
      freightCharges: 0,
      soapCharges: 0,
      distributorMargin: 0,
      retailerMargin: 0,
      leakageRate: 0,
      vatPercentage: 20,
      vatValue: 0,
      discount1Percentage: 0,
      discount1PercentageValue: 0,
      discount2PerPack: 0,
      discount2PerPackValue: 0,
      sgId: 0,
      stAmount17: 0,
      stAmount03: 0,
    };
  }

  private focusFirstEditableField(): void {
    this.cdr.detectChanges();
    const tryFocus = (): boolean => {
      const root = this.host.nativeElement as HTMLElement;
      const dateEl =
        this.salesEntryDateInput?.nativeElement ??
        (root.querySelector('.si__section--details input[type="date"]') as HTMLInputElement | null);
      if (dateEl && !dateEl.disabled) {
        dateEl.focus();
        return true;
      }
      const first = this.getFocusableFields()[0];
      if (first) {
        first.focus();
        return true;
      }
      return false;
    };

    requestAnimationFrame(() => {
      if (!tryFocus()) {
        this.cdr.detectChanges();
        requestAnimationFrame(() => {
          if (!tryFocus()) {
            setTimeout(() => tryFocus(), 80);
          }
        });
      }
    });
  }

  private buildDropdownStyle(anchor: HTMLElement, minWidthPx: number): Record<string, string> {
    const rect = anchor.getBoundingClientRect();
    const width = Math.min(
      Math.max(rect.width, minWidthPx || rect.width),
      window.innerWidth - 16,
    );
    let left = rect.left;
    if (left + width > window.innerWidth - 8) {
      left = Math.max(8, window.innerWidth - 8 - width);
    }
    return {
      top: `${rect.bottom + 4}px`,
      left: `${left}px`,
      width: `${width}px`,
      minWidth: `${width}px`,
      maxWidth: `${width}px`,
    };
  }

  private scrollActiveLookupOption(kind: 'customer' | 'fg'): void {
    setTimeout(() => {
      const root = this.host.nativeElement as HTMLElement;
      const listSelector =
        kind === 'customer' ? '.si__dropdown--customer' : '.si__dropdown--fg';
      const list = root.querySelector(listSelector) as HTMLElement | null;
      const el = list?.querySelector('li.is-active') as HTMLElement | null;
      if (!list || !el) {
        return;
      }
      const top = el.offsetTop;
      const bottom = top + el.offsetHeight;
      if (top < list.scrollTop) {
        list.scrollTop = top;
      } else if (bottom > list.scrollTop + list.clientHeight) {
        list.scrollTop = bottom - list.clientHeight;
      }
    }, 0);
  }

  private prefetchLookups(): void {
    this.prefetchSub?.unsubscribe();
    this.prefetchSub = new Subscription();
    this.prefetchSub.add(
      this.api.searchCustomers('', 100).subscribe({
        next: (rows) => {
          this.customerCache = rows;
        },
      }),
    );
    this.prefetchSub.add(
      this.api.searchFinishedGoods('', 100).subscribe({
        next: (rows) => {
          this.fgCache = rows;
        },
      }),
    );
  }

  private filterCustomerCache(term: string): CustomerLookup[] {
    if (!this.customerCache.length) {
      return [];
    }
    if (!term) {
      return this.customerCache.slice(0, 100);
    }
    const t = term.toLowerCase();
    return this.customerCache
      .filter(
        (c) =>
          c.customerId.toLowerCase().includes(t) ||
          c.customerName.toLowerCase().includes(t),
      )
      .slice(0, 100);
  }

  private filterFgCache(term: string): FinishedGoodLookup[] {
    if (!this.fgCache.length) {
      return [];
    }
    if (!term) {
      return this.fgCache.slice(0, 100);
    }
    const t = term.toLowerCase();
    return this.fgCache
      .filter(
        (fg) =>
          fg.fgId.toString().includes(t) ||
          fg.fgName.toLowerCase().includes(t) ||
          (fg.brandName ?? '').toLowerCase().includes(t),
      )
      .slice(0, 100);
  }

  private focusElement(el: HTMLElement): void {
    el.focus();
    if (el instanceof HTMLInputElement && el.type !== 'checkbox' && el.type !== 'date') {
      el.select();
    }
  }

  private focusFieldByName(name: string): void {
    const root = this.host.nativeElement as HTMLElement;
    const el = root.querySelector(`[data-si-field="${name}"]`) as HTMLElement | null;
    if (el) {
      this.focusElement(el);
    }
  }

  private focusGridField(rowIndex: number, field: 'fg' | 'packs' | 'price' | 'vat'): void {
    const root = this.host.nativeElement as HTMLElement;
    const el = root.querySelector(
      `[data-si-field="${field}"][data-row="${rowIndex}"]`,
    ) as HTMLElement | null;
    if (el) {
      this.focusElement(el);
    }
  }

  private getFocusableFields(): HTMLElement[] {
    const root = this.host.nativeElement as HTMLElement;
    const nodes = root.querySelectorAll('input:not([type="hidden"]), select, textarea');
    return Array.from(nodes).filter((el): el is HTMLElement => {
      if (!(el instanceof HTMLElement)) {
        return false;
      }
      if (el.closest('.si-find')) {
        return false;
      }
      const input = el as HTMLInputElement;
      if (input.disabled) {
        return false;
      }
      if (input.readOnly || input.classList.contains('is-readonly')) {
        return false;
      }
      return true;
    });
  }

  private applyUserPrivileges(rows: UserPrivilege[]): void {
    this.userCanPost = rows.some((p) => p.canPost);
    this.userCanCheck = rows.some((p) => p.canCheck);
  }

  private syncFinYearFromShell(): void {
    const stored = this.readStoredFinYear();
    if (stored) {
      this.finYear = stored;
    }
  }

  private readStoredFinYear(): string | null {
    try {
      const raw = sessionStorage.getItem(PERIOD_STORAGE_KEY);
      if (!raw) {
        return null;
      }
      const saved = JSON.parse(raw) as { finYear?: string };
      const fy = (saved.finYear || '').trim();
      return fy || null;
    } catch {
      return null;
    }
  }

  private readAccountingPeriodMeta(): {
    start: string;
    end: string;
    isFullYear: boolean;
  } | null {
    try {
      const raw = sessionStorage.getItem(PERIOD_STORAGE_KEY);
      if (!raw) {
        return null;
      }
      const saved = JSON.parse(raw) as { periodLabel?: string };
      const label = saved.periodLabel ?? '';
      const isFullYear = /full\s+financial\s+year/i.test(label);
      const match = label.match(
        /\((\d{2}\s+\w{3}\s+\d{4})\s*[\u2013\u2014-]\s*(\d{2}\s+\w{3}\s+\d{4})\)/,
      );
      if (!match) {
        return isFullYear ? { start: '', end: '', isFullYear: true } : null;
      }
      const start = this.parseUkDate(match[1]);
      const end = this.parseUkDate(match[2]);
      if (!start || !end) {
        return isFullYear ? { start: '', end: '', isFullYear: true } : null;
      }
      return {
        start: this.toInputDate(start),
        end: this.toInputDate(end),
        isFullYear,
      };
    } catch {
      return null;
    }
  }

  private parseUkDate(text: string): Date | null {
    const d = new Date(text.replace(/(\d{2}) (\w{3}) (\d{4})/, '$2 $1, $3'));
    return Number.isNaN(d.getTime()) ? null : d;
  }

  private toInputDate(d: Date): string {
    const yyyy = d.getFullYear();
    const mm = String(d.getMonth() + 1).padStart(2, '0');
    const dd = String(d.getDate()).padStart(2, '0');
    return `${yyyy}-${mm}-${dd}`;
  }
}

function round2(n: number): number {
  return Math.round((n + Number.EPSILON) * 100) / 100;
}

function round3(n: number): number {
  return Math.round((n + Number.EPSILON) * 1000) / 1000;
}
