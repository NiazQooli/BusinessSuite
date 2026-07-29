import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnDestroy, OnInit, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';
import { SalesInvoice, SalesInvoiceLine } from '../../core/models/sales-invoice.model';
import { AuthService } from '../../core/services/auth.service';
import { readInvoicePrintCache } from '../../core/services/invoice-print-cache';
import { SalesInvoiceService } from '../../core/services/sales-invoice.service';

/** Matches VB ActiveReport org header (report designer static text). */
const COMPANY = {
  name: 'FRESHER ESSENTIALS LIMITED',
  addressLine1: 'OFFICE 1, HASLAM HOUSE, 105 CHORLEY OLD ROAD, BOLTON BL1 3AS,',
  addressLine2: 'UNITED KINGDOM.',
};

const TOKEN_KEY = 'fresher.accessToken';
const SESSION_KEY = 'fresher.session';

type PrintRow =
  | { kind: 'brand'; brand: string }
  | { kind: 'item'; line: SalesInvoiceLine };

export interface InvoicePrintPage {
  pageNo: number;
  rows: PrintRow[];
  showTotals: boolean;
}

@Component({
  selector: 'app-invoice-print',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './invoice-print.component.html',
  styleUrl: './invoice-print.component.css',
})
export class InvoicePrintComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(SalesInvoiceService);
  private readonly auth = inject(AuthService);
  private readonly cdr = inject(ChangeDetectorRef);
  private sub?: Subscription;

  readonly company = COMPANY;

  loading = true;
  error = '';
  invoice: SalesInvoice | null = null;
  pages: InvoicePrintPage[] = [];
  printedAt = new Date();
  printedBy = 'USER';
  private autoPrint = false;

  ngOnInit(): void {
    document.body.classList.add('invoice-print-body');
    this.syncAuthFromOpener();

    const id = Number(this.route.snapshot.paramMap.get('id'));
    const finYear = (this.route.snapshot.queryParamMap.get('finYear') || '').trim();
    this.autoPrint = this.route.snapshot.queryParamMap.get('autoPrint') === '1';

    const session = this.auth.getSession();
    this.printedBy = (
      session?.fullName ||
      session?.userName ||
      session?.userId ||
      'USER'
    ).toUpperCase();

    if (!id || !finYear) {
      this.loading = false;
      this.error = 'Missing invoice id or financial year.';
      this.cdr.detectChanges();
      return;
    }

    // Preview tab: use snapshot written by Sales Invoice (avoids sessionStorage auth gap).
    const cached = readInvoicePrintCache(id, finYear);
    if (cached) {
      if (cached.printedBy) {
        this.printedBy = cached.printedBy.toUpperCase();
      }
      this.applyInvoice(cached.invoice);
      return;
    }

    this.sub = this.api.getById(id, finYear).subscribe({
      next: (inv) => this.applyInvoice(inv),
      error: (err) => {
        this.loading = false;
        this.error =
          err?.error?.detail ||
          err?.error?.title ||
          (err?.status === 401
            ? 'Please sign in again, then reopen Preview.'
            : err?.status === 404
              ? 'Sales invoice not found.'
              : 'Failed to load sales invoice.');
        this.cdr.detectChanges();
      },
    });
  }

  ngOnDestroy(): void {
    document.body.classList.remove('invoice-print-body');
    this.sub?.unsubscribe();
  }

  print(): void {
    window.print();
  }

  get invoiceNo(): string {
    if (!this.invoice) {
      return '';
    }
    const type = (this.invoice.salesEntryType || 'BG').trim() || 'BG';
    return `${type} - ${String(this.invoice.salesEntryId).padStart(6, '0')}`;
  }

  get invoiceDate(): string {
    if (!this.invoice?.salesEntryDate) {
      return '';
    }
    const d = new Date(this.invoice.salesEntryDate);
    if (Number.isNaN(d.getTime())) {
      return this.invoice.salesEntryDate;
    }
    return this.formatDate(d);
  }

  get buyerCode(): string {
    return this.invoice?.customerId?.trim() || '';
  }

  get buyerName(): string {
    return this.invoice?.customerName?.trim() || '';
  }

  get buyerAddress(): string {
    const a = this.invoice?.address?.trim();
    return a ? a : 'N/A';
  }

  get vatHeader(): string {
    const rates = (this.invoice?.lines ?? [])
      .map((l) => Number(l.vatPercentage) || 0)
      .filter((v) => v > 0);
    const rate = rates[0] ?? 20;
    return `VAT ${rate}% (£)`;
  }

  get totalPacks(): number {
    return this.itemLines().reduce((sum, l) => sum + (Number(l.noOfPacks) || 0), 0);
  }

  get subTotal(): number {
    return this.itemLines().reduce((sum, l) => sum + (Number(l.excStValue) || 0), 0);
  }

  get grandTotal(): number {
    return Number(this.invoice?.netAmount) || this.subTotal;
  }

  get printedAtLabel(): string {
    return this.formatDateTime(this.printedAt);
  }

  formatVat(value: number): string {
    if (!value) {
      return '-';
    }
    return this.formatMoney(value);
  }

  formatPrice(value: number | null | undefined): string {
    const n = Number(value) || 0;
    const hasFraction = Math.abs(n % 1) >= 0.005;
    return this.formatMoney(n, hasFraction ? 2 : 0);
  }

  formatMoney(value: number | string | null | undefined, fractionDigits = 2): string {
    const n = Number(value);
    if (!Number.isFinite(n)) {
      return (0).toLocaleString('en-GB', {
        minimumFractionDigits: fractionDigits,
        maximumFractionDigits: fractionDigits,
      });
    }
    return n.toLocaleString('en-GB', {
      minimumFractionDigits: fractionDigits,
      maximumFractionDigits: fractionDigits,
    });
  }

  private applyInvoice(inv: SalesInvoice): void {
    try {
      this.invoice = inv;
      this.pages = this.buildPages(inv);
      this.loading = false;
      this.error = '';
      this.cdr.detectChanges();
      if (this.autoPrint) {
        setTimeout(() => this.print(), 400);
      }
    } catch (e) {
      this.loading = false;
      this.error = e instanceof Error ? e.message : 'Failed to render invoice.';
      this.cdr.detectChanges();
    }
  }

  /**
   * New tab does not inherit sessionStorage. Copy auth from opener so API calls work
   * when the user signed in without "Remember me".
   */
  private syncAuthFromOpener(): void {
    if (this.auth.getAccessToken()) {
      return;
    }
    try {
      const opener = window.opener as Window | null;
      if (!opener || opener.closed) {
        return;
      }
      const token =
        opener.sessionStorage.getItem(TOKEN_KEY) ?? opener.localStorage.getItem(TOKEN_KEY);
      const session =
        opener.sessionStorage.getItem(SESSION_KEY) ?? opener.localStorage.getItem(SESSION_KEY);
      if (token) {
        sessionStorage.setItem(TOKEN_KEY, token);
        if (session) {
          sessionStorage.setItem(SESSION_KEY, session);
        }
      }
    } catch {
      /* opener inaccessible */
    }
  }

  private itemLines(): SalesInvoiceLine[] {
    return (this.invoice?.lines ?? []).filter((l) => l.fgId && (l.noOfPacks ?? 0) > 0);
  }

  private buildRows(inv: SalesInvoice): PrintRow[] {
    const lines = (inv.lines ?? [])
      .filter((l) => l.fgId && (l.noOfPacks ?? 0) > 0)
      .slice()
      .sort((a, b) => {
        const brandCmp = (a.brandName || '').localeCompare(b.brandName || '');
        if (brandCmp !== 0) {
          return brandCmp;
        }
        return (a.fgId ?? 0) - (b.fgId ?? 0);
      });

    const rows: PrintRow[] = [];
    let lastBrand = '';
    for (const line of lines) {
      const brand = (line.brandName || '').trim() || 'GENERAL';
      if (brand !== lastBrand) {
        rows.push({ kind: 'brand', brand });
        lastBrand = brand;
      }
      rows.push({ kind: 'item', line });
    }
    return rows;
  }

  /** VB multi-page: footer every page; totals only on last page. */
  private buildPages(inv: SalesInvoice): InvoicePrintPage[] {
    const rows = this.buildRows(inv);
    const firstPageCap = 28;
    const otherPageCap = 38;

    if (rows.length === 0) {
      return [{ pageNo: 1, rows: [], showTotals: true }];
    }

    const chunks: PrintRow[][] = [];
    let i = 0;
    let pageIndex = 0;
    while (i < rows.length) {
      const cap = pageIndex === 0 ? firstPageCap : otherPageCap;
      let end = Math.min(i + cap, rows.length);
      if (end < rows.length && rows[end - 1]?.kind === 'brand') {
        end -= 1;
      }
      if (end <= i) {
        end = i + 1;
      }
      chunks.push(rows.slice(i, end));
      i = end;
      pageIndex += 1;
    }

    const totalPages = chunks.length;
    return chunks.map((chunk, idx) => ({
      pageNo: idx + 1,
      rows: chunk,
      showTotals: idx === totalPages - 1,
    }));
  }

  private formatDate(d: Date): string {
    const dd = String(d.getDate()).padStart(2, '0');
    const mm = String(d.getMonth() + 1).padStart(2, '0');
    const yyyy = d.getFullYear();
    return `${dd}/${mm}/${yyyy}`;
  }

  private formatDateTime(d: Date): string {
    const date = this.formatDate(d);
    let hours = d.getHours();
    const minutes = String(d.getMinutes()).padStart(2, '0');
    const ampm = hours >= 12 ? 'PM' : 'AM';
    hours = hours % 12 || 12;
    return `${date} at ${hours}:${minutes} ${ampm}`;
  }
}
