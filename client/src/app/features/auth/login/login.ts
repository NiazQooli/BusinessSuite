import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class LoginComponent {
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);

  userName = '';
  password = '';
  remember = true;

  readonly submitting = signal(false);
  readonly error = signal<string | null>(null);
  readonly showPassword = signal(false);

  togglePassword(): void {
    this.showPassword.update((v) => !v);
  }

  submit(): void {
    this.error.set(null);

    const userName = this.userName.trim();
    if (!userName || !this.password) {
      this.error.set('Enter your user name and password to continue.');
      return;
    }

    this.submitting.set(true);

    this.authService.login({ userName, password: this.password }, this.remember).subscribe({
      next: () => {
        this.submitting.set(false);
        void this.router.navigateByUrl('/dashboard');
      },
      error: (err: HttpErrorResponse) => {
        this.submitting.set(false);
        this.error.set(this.readError(err));
      },
    });
  }

  private readError(err: HttpErrorResponse): string {
    const body = err.error as
      | { title?: string; errors?: Record<string, string[]>; detail?: string }
      | null;

    const firstFieldError = body?.errors
      ? Object.values(body.errors).flat().find(Boolean)
      : undefined;

    if (firstFieldError) {
      return firstFieldError;
    }

    if (err.status === 401) {
      return 'Invalid user name or password.';
    }

    if (err.status === 0) {
      return 'Unable to reach the API. Start the .NET backend first.';
    }

    return body?.detail || body?.title || 'Sign in failed. Please try again.';
  }
}
