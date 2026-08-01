import { CommonModule } from '@angular/common';
import { Component, HostListener, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { AuthService } from '../core/services/auth.service';
import { SalesInvoiceService } from '../core/services/sales-invoice.service';

export interface ShellNavItem {
  label: string;
  route: string;
}

export interface ShellNavGroup {
  id: string;
  label: string;
  items: ShellNavItem[];
}

const PERIOD_KEY = 'fresher.accountingPeriod';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './shell.html',
  styleUrl: './shell.scss',
})
export class ShellComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly salesInvoiceApi = inject(SalesInvoiceService);

  /** Add future main menus and child items here. */
  readonly navGroups: ShellNavGroup[] = [
    {
      id: 'sales',
      label: 'Sales',
      items: [{ label: 'Sales Invoice', route: '/sales-invoice' }],
    },
  ];

  readonly expandedGroups = signal<Record<string, boolean>>({ sales: false });
  readonly userMenuOpen = signal(false);
  readonly periodModalOpen = signal(false);
  readonly sidebarCollapsed = signal(false);

  private readonly sidebarKey = 'fresher.sidebarCollapsed';

  displayName = 'USER';
  userInitial = 'U';
  finYear = '2026-27';
  periodLabel = 'Full Financial Year (01 Jul 2026 – 30 Jun 2027)';

  /** Temporary options until dedicated accounting-period API is wired. */
  financialYearOptions = [
    {
      value: '2027-28',
      label: '2027-28 (01/07/2027 - 30/06/2028)',
      period: 'Full Financial Year (01 Jul 2027 – 30 Jun 2028)',
    },
    {
      value: '2026-27',
      label: '2026-27 (01/07/2026 - 30/06/2027)',
      period: 'Full Financial Year (01 Jul 2026 – 30 Jun 2027)',
    },
    {
      value: '2025-26',
      label: '2025-26 (01/07/2025 - 30/06/2026)',
      period: 'Full Financial Year (01 Jul 2025 – 30 Jun 2026)',
    },
    {
      value: '2024-25',
      label: '2024-25 (01/07/2024 - 30/06/2025)',
      period: 'Full Financial Year (01 Jul 2024 – 30 Jun 2025)',
    },
  ];

  selectedFinYear = '2026-27';
  selectedPeriod = 'Full Financial Year (01 Jul 2026 – 30 Jun 2027)';

  ngOnInit(): void {
    const session = this.authService.getSession();
    const name = (session?.fullName || session?.userName || session?.userId || 'USER').trim();
    this.displayName = name.toUpperCase();
    this.userInitial = this.displayName.charAt(0) || 'U';

    this.restorePeriod();
    this.restoreSidebarState();
    this.loadFinYearFromApi();

    this.syncSalesExpansion(this.router.url);
    this.router.events
      .pipe(filter((e) => e instanceof NavigationEnd))
      .subscribe((e) => this.syncSalesExpansion((e as NavigationEnd).urlAfterRedirects));
  }

  isGroupActive(group: ShellNavGroup): boolean {
    const url = this.router.url;
    return group.items.some((item) => url.startsWith(item.route));
  }

  isGroupExpanded(groupId: string): boolean {
    return this.expandedGroups()[groupId] ?? false;
  }

  toggleGroup(groupId: string): void {
    this.expandedGroups.update((state) => ({
      ...state,
      [groupId]: !state[groupId],
    }));
  }

  toggleSidebar(): void {
    this.sidebarCollapsed.update((collapsed) => {
      const next = !collapsed;
      localStorage.setItem(this.sidebarKey, next ? '1' : '0');
      return next;
    });
  }

  private restoreSidebarState(): void {
    this.sidebarCollapsed.set(localStorage.getItem(this.sidebarKey) === '1');
  }

  toggleUserMenu(event: MouseEvent): void {
    event.stopPropagation();
    this.userMenuOpen.update((open) => !open);
  }

  closeUserMenu(): void {
    this.userMenuOpen.set(false);
  }

  @HostListener('document:click')
  onDocumentClick(): void {
    this.closeUserMenu();
  }

  signOut(): void {
    this.closeUserMenu();
    this.authService.logout();
    void this.router.navigateByUrl('/login');
  }

  openChangePeriod(): void {
    this.closeUserMenu();
    this.selectedFinYear = this.finYear;
    this.selectedPeriod = this.periodLabel;
    this.periodModalOpen.set(true);
  }

  closePeriodModal(): void {
    this.periodModalOpen.set(false);
  }

  onFinYearChange(value: string): void {
    this.selectedFinYear = value;
    const match = this.financialYearOptions.find((o) => o.value === value);
    if (match) {
      this.selectedPeriod = match.period;
    }
  }

  applyPeriod(): void {
    this.finYear = this.selectedFinYear;
    this.periodLabel = this.selectedPeriod;
    localStorage.setItem(
      PERIOD_KEY,
      JSON.stringify({ finYear: this.finYear, periodLabel: this.periodLabel }),
    );
    // Clear old session copy so it cannot override on next load.
    sessionStorage.removeItem(PERIOD_KEY);
    this.periodModalOpen.set(false);
  }

  private restorePeriod(): void {
    const raw = localStorage.getItem(PERIOD_KEY) ?? sessionStorage.getItem(PERIOD_KEY);
    if (!raw) {
      return;
    }
    try {
      const saved = JSON.parse(raw) as { finYear?: string; periodLabel?: string };
      if (saved.finYear) {
        this.finYear = saved.finYear;
        this.selectedFinYear = saved.finYear;
      }
      if (saved.periodLabel) {
        this.periodLabel = saved.periodLabel;
        this.selectedPeriod = saved.periodLabel;
      }
      // Migrate session → local so it survives browser restart.
      localStorage.setItem(PERIOD_KEY, raw);
      sessionStorage.removeItem(PERIOD_KEY);
    } catch {
      /* ignore */
    }
  }

  private loadFinYearFromApi(): void {
    if (!this.authService.getAccessToken()) {
      return;
    }
    this.salesInvoiceApi.getContext().subscribe({
      next: (ctx) => {
        const hasSaved =
          !!localStorage.getItem(PERIOD_KEY) || !!sessionStorage.getItem(PERIOD_KEY);
        if (!hasSaved && ctx.finYear) {
          this.finYear = ctx.finYear;
          this.selectedFinYear = ctx.finYear;
          const match = this.financialYearOptions.find((o) => o.value === ctx.finYear);
          if (match) {
            this.periodLabel = match.period;
            this.selectedPeriod = match.period;
          } else {
            this.periodLabel = `Full Financial Year (${ctx.finYear})`;
            this.selectedPeriod = this.periodLabel;
          }
        }
      },
      error: () => {
        /* keep defaults for UI approval */
      },
    });
  }

  private syncSalesExpansion(url: string): void {
    // Expand Sales only while a Sales child screen is open; otherwise keep collapsed.
    const onSalesScreen = url.includes('/sales-invoice');
    this.expandedGroups.update((state) => ({ ...state, sales: onSalesScreen }));
  }
}
