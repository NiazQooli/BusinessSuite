import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { UserPrivilege } from '../models/privilege.model';

@Injectable({ providedIn: 'root' })
export class PrivilegeService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/privileges`;

  /** Loads tblAdmUserPrivilege rows for the signed-in user. */
  getMine(): Observable<UserPrivilege[]> {
    return this.http.get<UserPrivilege[]>(`${this.baseUrl}/me`);
  }
}
