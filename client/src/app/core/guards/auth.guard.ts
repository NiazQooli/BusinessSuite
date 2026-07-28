import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/** Requires a stored access token; otherwise redirects to login. */
export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.getAccessToken()) {
    return true;
  }

  return router.createUrlTree(['/login']);
};

/** Login page only when signed out. */
export const guestGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.getAccessToken()) {
    return true;
  }

  return router.createUrlTree(['/dashboard']);
};
