import { Injectable, signal } from '@angular/core';
import { Toast, ToastType } from '../models/toast.model';

const DEFAULT_DURATION_MS = 2800;

@Injectable({ providedIn: 'root' })
export class ToastService {
  private nextId = 1;
  private readonly timers = new Map<number, ReturnType<typeof setTimeout>>();

  /** Active toasts (newest last — rendered bottom of stack). */
  readonly toasts = signal<Toast[]>([]);

  success(message: string, title = 'Success', durationMs = DEFAULT_DURATION_MS): void {
    this.show('success', message, title, durationMs);
  }

  error(message: string, title = 'Error', durationMs = DEFAULT_DURATION_MS): void {
    this.show('error', message, title, durationMs);
  }

  info(message: string, title = 'Info', durationMs = DEFAULT_DURATION_MS): void {
    this.show('info', message, title, durationMs);
  }

  warning(message: string, title = 'Warning', durationMs = DEFAULT_DURATION_MS): void {
    this.show('warning', message, title, durationMs);
  }

  dismiss(id: number): void {
    const timer = this.timers.get(id);
    if (timer) {
      clearTimeout(timer);
      this.timers.delete(id);
    }
    this.toasts.update((list) => list.filter((t) => t.id !== id));
  }

  clear(): void {
    for (const timer of this.timers.values()) {
      clearTimeout(timer);
    }
    this.timers.clear();
    this.toasts.set([]);
  }

  private show(type: ToastType, message: string, title: string, durationMs: number): void {
    const id = this.nextId++;
    const toast: Toast = { id, type, title, message, durationMs };
    this.toasts.update((list) => [...list, toast].slice(-5));

    if (durationMs > 0) {
      const timer = setTimeout(() => this.dismiss(id), durationMs);
      this.timers.set(id, timer);
    }
  }
}
