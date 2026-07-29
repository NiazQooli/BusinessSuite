import { ApplicationConfig, LOCALE_ID, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { registerLocaleData } from '@angular/common';
import localeEnGb from '@angular/common/locales/en-GB';
import { provideNgIconsConfig } from '@ng-icons/core';

import { routes } from './app.routes';
import { apiInterceptor } from './core/interceptors/api.interceptor';

registerLocaleData(localeEnGb);

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([apiInterceptor])),
    { provide: LOCALE_ID, useValue: 'en-GB' },
    provideNgIconsConfig({
      size: '1.05rem',
      strokeWidth: 2.15,
    }),
  ],
};
