import { createReducer, on } from '@ngrx/store';
import * as OrderActions from './order.actions';

export interface OrderState {
  orderStatus: string;
  isLoading: boolean;
}

export const initialState: OrderState = {
  orderStatus: '',
  isLoading: false,
};

export const orderReducer = createReducer(
  initialState,
  on(OrderActions.submitOrder, (state) => ({ ...state, isLoading: true, orderStatus: 'Submitting...' })),
  on(OrderActions.submitOrderSuccess, (state, { orderId }) => ({
    ...state,
    isLoading: false,
    orderStatus: `Success! Order ID: ${orderId} created.`,
  })),
  on(OrderActions.submitOrderFailure, (state, { error }) => ({
    ...state,
    isLoading: false,
    orderStatus: `Error: ${error}`,
  }))
);
