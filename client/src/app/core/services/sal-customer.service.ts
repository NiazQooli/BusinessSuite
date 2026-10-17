import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  PaginatedSalCustomers,
  SalCustomer,
  SaveSalCustomerRequest,
} from '../models/sal-customer.model';

@Injectable({ providedIn: 'root' })
export class SalCustomerService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/sal-customers`;

  search(pageNumber = 1, pageSize = 500, search?: string): Observable<PaginatedSalCustomers> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);

    if (search?.trim()) {
      params = params.set('search', search.trim());
    }

    return this.http.get<PaginatedSalCustomers>(this.baseUrl, { params });
  }

  getById(customerId: string): Observable<SalCustomer> {
    return this.http.get<SalCustomer>(`${this.baseUrl}/${encodeURIComponent(customerId)}`);
  }

  create(request: SaveSalCustomerRequest): Observable<SalCustomer> {
    return this.http.post<SalCustomer>(this.baseUrl, request);
  }

  update(customerId: string, request: SaveSalCustomerRequest): Observable<SalCustomer> {
    return this.http.put<SalCustomer>(
      `${this.baseUrl}/${encodeURIComponent(customerId)}`,
      request,
    );
  }

  delete(customerId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${encodeURIComponent(customerId)}`);
  }
}
