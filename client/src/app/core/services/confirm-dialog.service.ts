import { Injectable, signal } from '@angular/core';
import { ConfirmOptions, ConfirmRequest } from '../models/confirm.model';

@Injectable({ providedIn: 'root' })
export class ConfirmDialogService {
  private nextId = 1;
  readonly active = signal<ConfirmRequest | null>(null);

  /** Opens a styled confirm dialog. Resolves true = OK, false = Cancel. */
  ask(messageOrOptions: string | ConfirmOptions): Promise<boolean> {
    const options: ConfirmOptions =
      typeof messageOrOptions === 'string'
        ? { message: messageOrOptions }
        : messageOrOptions;

    // Close any previous dialog as cancelled
    const previous = this.active();
    if (previous) {
      previous.resolve(false);
    }

    return new Promise<boolean>((resolve) => {
      this.active.set({
        id: this.nextId++,
        title: options.title ?? 'Please confirm',
        message: options.message,
        confirmText: options.confirmText ?? 'OK',
        cancelText: options.cancelText ?? 'Cancel',
        tone: options.tone ?? 'default',
        resolve,
      });
    });
  }

  confirm(): void {
    const current = this.active();
    if (!current) {
      return;
    }
    this.active.set(null);
    current.resolve(true);
  }

  cancel(): void {
    const current = this.active();
    if (!current) {
      return;
    }
    this.active.set(null);
    current.resolve(false);
  }
}
