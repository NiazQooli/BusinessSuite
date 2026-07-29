import { Routes } from '@angular/router';
import { ShellComponent } from './layout/shell';
import { authGuard, guestGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/login/login').then((m) => m.LoginComponent),
  },
  /* Printable A4 invoice — outside shell (no sidebar / navbar) */
  {
    path: 'print/invoice/:id',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/invoice-print/invoice-print.component').then(
        (m) => m.InvoicePrintComponent,
      ),
  },
  {
    path: '',
    component: ShellComponent,
    canActivate: [authGuard],
    children: [
      {
        path: '',
        pathMatch: 'full',
        redirectTo: 'dashboard',
      },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/dashboard/dashboard').then((m) => m.DashboardComponent),
      },
      {
        path: 'sales-invoice',
        loadComponent: () =>
          import('./features/sales-invoice/sales-invoice').then((m) => m.SalesInvoiceComponent),
      },
      {
        path: 'customers',
        loadComponent: () =>
          import('./features/customers/customers').then((m) => m.CustomersComponent),
      },
    ],
  },
  { path: '**', redirectTo: 'login' },
];
