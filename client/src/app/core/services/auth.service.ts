import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface LoginRequest {
  userName: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  expiresAtUtc: string;
  userId: string;
  companyId: number;
  userName: string;
  fullName: string;
}

const TOKEN_KEY = 'fresher.accessToken';
const SESSION_KEY = 'fresher.session';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/auth`;

  login(request: LoginRequest, remember: boolean): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.baseUrl}/login`, request).pipe(
      tap((response) => this.persistSession(response, remember)),
    );
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    sessionStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(SESSION_KEY);
    sessionStorage.removeItem(SESSION_KEY);
  }

  getAccessToken(): string | null {
    return localStorage.getItem(TOKEN_KEY) ?? sessionStorage.getItem(TOKEN_KEY);
  }

  private persistSession(response: LoginResponse, remember: boolean): void {
    const storage = remember ? localStorage : sessionStorage;
    const other = remember ? sessionStorage : localStorage;

    other.removeItem(TOKEN_KEY);
    other.removeItem(SESSION_KEY);

    storage.setItem(TOKEN_KEY, response.accessToken);
    storage.setItem(
      SESSION_KEY,
      JSON.stringify({
        userId: response.userId,
        userName: response.userName,
        companyId: response.companyId,
        fullName: response.fullName,
        expiresAtUtc: response.expiresAtUtc,
      }),
    );
  }
}
