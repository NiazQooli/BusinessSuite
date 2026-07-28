import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { ToastService } from '../services/toast.service';

const TOKEN_KEY = 'fresher.accessToken';

let handlingUnauthorized = false;

export const apiInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const toast = inject(ToastService);

  const token = localStorage.getItem(TOKEN_KEY) ?? sessionStorage.getItem(TOKEN_KEY);

  const headers: Record<string, string> = {
    Accept: 'application/json',
  };

  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  return next(req.clone({ setHeaders: headers })).pipe(
    catchError((err: unknown) => {
      if (err instanceof HttpErrorResponse && err.status === 401) {
        const isLoginCall = req.url.includes('/auth/login');
        if (!isLoginCall && !handlingUnauthorized) {
          handlingUnauthorized = true;
          auth.logout();
          toast.error('Please sign in to continue.', 'Session required');
          void router.navigateByUrl('/login').finally(() => {
            handlingUnauthorized = false;
          });
        }
      }
      return throwError(() => err);
    }),
  );
};
