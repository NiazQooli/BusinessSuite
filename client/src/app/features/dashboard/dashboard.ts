import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class DashboardComponent {
  private readonly auth = inject(AuthService);

  readonly displayName =
    this.auth.getSession()?.fullName ||
    this.auth.getSession()?.userName ||
    this.auth.getSession()?.userId ||
    'User';
}
