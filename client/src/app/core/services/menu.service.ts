import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { MenuItem } from '../models/menu-item.model';

@Injectable({ providedIn: 'root' })
export class MenuService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/menus`;

  /** Loads tblSftMenuItems for the signed-in user's company. */
  getForCompany(includeInactive = false): Observable<MenuItem[]> {
    const params = new HttpParams().set('includeInactive', includeInactive);
    return this.http.get<MenuItem[]>(this.baseUrl, { params });
  }
}
