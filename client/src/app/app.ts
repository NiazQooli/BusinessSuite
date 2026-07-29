import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ToastContainerComponent } from './shared/toast/toast-container';
import { ConfirmDialogComponent } from './shared/confirm-dialog/confirm-dialog';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, ToastContainerComponent, ConfirmDialogComponent],
  template: `
    <router-outlet />
    <app-toast-container />
    <app-confirm-dialog />
  `,
  styles: [
    `
      :host {
        display: block;
        height: 100%;
        max-height: 100%;
        overflow: hidden;
      }
    `,
  ],
})
export class App {}
