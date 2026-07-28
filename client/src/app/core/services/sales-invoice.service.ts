import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CustomerLookup,
  FinishedGoodLookup,
  SalesInvoice,
  SalesInvoiceContext,
  SalesInvoiceListItem,
  SaveSalesInvoiceRequest,
  SalesInvoiceSearchParams,
} from '../models/sales-invoice.model';

@Injectable({ providedIn: 'root' })
export class SalesInvoiceService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/sales-invoices`;

  getContext(): Observable<SalesInvoiceContext> {
    return this.http.get<SalesInvoiceContext>(`${this.baseUrl}/context`);
  }

  search(params: SalesInvoiceSearchParams): Observable<SalesInvoiceListItem[]> {
    let httpParams = new HttpParams()
      .set('finYear', params.finYear)
      .set('take', params.take ?? 50);
    if (params.search?.trim()) {
      httpParams = httpParams.set('search', params.search.trim());
    }
    if (params.searchField) {
      httpParams = httpParams.set('searchField', params.searchField);
    }
    if (params.sortOn) {
      httpParams = httpParams.set('sortOn', params.sortOn);
    }
    if (params.sortFlag !== undefined) {
      httpParams = httpParams.set('sortFlag', params.sortFlag);
    }
    if (params.periodStart) {
      httpParams = httpParams.set('periodStart', params.periodStart);
    }
    if (params.periodEnd) {
      httpParams = httpParams.set('periodEnd', params.periodEnd);
    }
    return this.http.get<SalesInvoiceListItem[]>(this.baseUrl, { params: httpParams });
  }

  getById(salesEntryId: number, finYear: string): Observable<SalesInvoice> {
    const params = new HttpParams().set('finYear', finYear);
    return this.http.get<SalesInvoice>(`${this.baseUrl}/${salesEntryId}`, { params });
  }

  create(request: SaveSalesInvoiceRequest): Observable<SalesInvoice> {
    return this.http.post<SalesInvoice>(this.baseUrl, request);
  }

  update(salesEntryId: number, request: SaveSalesInvoiceRequest): Observable<SalesInvoice> {
    return this.http.put<SalesInvoice>(`${this.baseUrl}/${salesEntryId}`, request);
  }

  delete(salesEntryId: number, finYear: string): Observable<void> {
    const params = new HttpParams().set('finYear', finYear);
    return this.http.delete<void>(`${this.baseUrl}/${salesEntryId}`, { params });
  }

  check(salesEntryId: number, finYear: string): Observable<void> {
    const params = new HttpParams().set('finYear', finYear);
    return this.http.post<void>(`${this.baseUrl}/${salesEntryId}/check`, {}, { params });
  }

  post(salesEntryId: number, finYear: string): Observable<void> {
    const params = new HttpParams().set('finYear', finYear);
    return this.http.post<void>(`${this.baseUrl}/${salesEntryId}/post`, {}, { params });
  }

  searchCustomers(search?: string, take = 30): Observable<CustomerLookup[]> {
    let params = new HttpParams().set('take', take);
    if (search?.trim()) {
      params = params.set('search', search.trim());
    }
    return this.http.get<CustomerLookup[]>(`${this.baseUrl}/lookups/customers`, { params });
  }

  searchFinishedGoods(search?: string, take = 30): Observable<FinishedGoodLookup[]> {
    let params = new HttpParams().set('take', take);
    if (search?.trim()) {
      params = params.set('search', search.trim());
    }
    return this.http.get<FinishedGoodLookup[]>(`${this.baseUrl}/lookups/finished-goods`, {
      params,
    });
  }
}
