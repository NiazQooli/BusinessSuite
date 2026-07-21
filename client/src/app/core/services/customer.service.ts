import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreateCustomerRequest,
  Customer,
  PaginatedList,
  UpdateCustomerRequest,
} from '../models/customer.model';

@Injectable({ providedIn: 'root' })
export class CustomerService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/customers`;

  getCustomers(
    pageNumber = 1,
    pageSize = 10,
    search?: string,
  ): Observable<PaginatedList<Customer>> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);

    if (search?.trim()) {
      params = params.set('search', search.trim());
    }

    return this.http.get<PaginatedList<Customer>>(this.baseUrl, { params });
  }

  getCustomer(id: string): Observable<Customer> {
    return this.http.get<Customer>(`${this.baseUrl}/${id}`);
  }

  createCustomer(request: CreateCustomerRequest): Observable<Customer> {
    return this.http.post<Customer>(this.baseUrl, request);
  }

  updateCustomer(request: UpdateCustomerRequest): Observable<Customer> {
    return this.http.put<Customer>(`${this.baseUrl}/${request.id}`, request);
  }

  deleteCustomer(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
