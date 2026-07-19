import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideStore } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';
import { provideStoreDevtools } from '@ngrx/store-devtools';

import { routes } from './app.routes';
import { orderReducer } from './store/order.reducer';
import { OrderEffects } from './store/order.effects';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(),
    provideStore({ order: orderReducer }),
    provideEffects([OrderEffects]),
    provideStoreDevtools({ maxAge: 25, logOnly: false })
  ]
};
