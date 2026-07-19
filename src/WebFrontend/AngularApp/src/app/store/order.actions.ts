import { createAction, props } from '@ngrx/store';

export const submitOrder = createAction(
  '[Order] Submit Order',
  props<{ customerName: string; totalAmount: number }>()
);

export const submitOrderSuccess = createAction(
  '[Order] Submit Order Success',
  props<{ orderId: string }>()
);

export const submitOrderFailure = createAction(
  '[Order] Submit Order Failure',
  props<{ error: string }>()
);
