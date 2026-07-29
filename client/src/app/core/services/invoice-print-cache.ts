import { SalesInvoice } from '../models/sales-invoice.model';

/** Shared between Sales Invoice Preview and /print/invoice/:id (new tab). */
export const INVOICE_PRINT_CACHE_KEY = 'fresher.invoicePrint.cache';

export interface InvoicePrintCache {
  salesEntryId: number;
  finYear: string;
  invoice: SalesInvoice;
  printedBy?: string;
  cachedAt: string;
}

export function writeInvoicePrintCache(cache: InvoicePrintCache): void {
  try {
    localStorage.setItem(INVOICE_PRINT_CACHE_KEY, JSON.stringify(cache));
  } catch {
    /* quota / private mode */
  }
}

export function readInvoicePrintCache(
  salesEntryId: number,
  finYear: string,
): InvoicePrintCache | null {
  try {
    const raw = localStorage.getItem(INVOICE_PRINT_CACHE_KEY);
    if (!raw) {
      return null;
    }
    const cache = JSON.parse(raw) as InvoicePrintCache;
    if (
      cache?.salesEntryId === salesEntryId &&
      (cache.finYear || '').trim() === finYear.trim() &&
      cache.invoice
    ) {
      return cache;
    }
  } catch {
    /* ignore */
  }
  return null;
}
