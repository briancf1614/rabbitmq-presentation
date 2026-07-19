import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { HttpClient } from '@angular/common/http';
import { catchError, map, mergeMap, of } from 'rxjs';
import * as OrderActions from './order.actions';

@Injectable()
export class OrderEffects {
  private actions$ = inject(Actions);
  private http = inject(HttpClient);

  submitOrder$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OrderActions.submitOrder),
      mergeMap(action =>
        this.http.post<{ orderId: string }>('http://localhost:5000/api/orders', {
          customerName: action.customerName,
          totalAmount: action.totalAmount,
        }).pipe(
          map(response => OrderActions.submitOrderSuccess({ orderId: response.orderId })),
          catchError(error => {
            let errorMsg = error.message;
            if (error.error && error.error.messages) {
               errorMsg = error.error.messages.join(', ');
            }
            return of(OrderActions.submitOrderFailure({ error: errorMsg }));
          })
        )
      )
    )
  );
}
