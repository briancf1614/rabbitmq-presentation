import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { submitOrder } from './store/order.actions';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div style="padding: 20px; font-family: Arial, sans-serif;">
      <h1>Enterprise Dashboard (NgRx & SignalR)</h1>

      <div style="border: 1px solid #ccc; padding: 15px; margin-bottom: 20px; border-radius: 5px;">
        <h2>Create New Order</h2>
        <div style="margin-bottom: 10px;">
          <label>Customer Name:</label><br>
          <input [(ngModel)]="customerName" type="text" style="width: 300px; padding: 5px;" />
        </div>
        <div style="margin-bottom: 10px;">
          <label>Total Amount:</label><br>
          <input [(ngModel)]="totalAmount" type="number" style="width: 300px; padding: 5px;" />
        </div>
        <button (click)="submitOrder()" [disabled]="(isLoading$ | async)" style="padding: 10px 15px; background: #007bff; color: white; border: none; border-radius: 3px; cursor: pointer;">
          {{ (isLoading$ | async) ? 'Submitting...' : 'Place Order' }}
        </button>
      </div>

      <div *ngIf="orderStatus$ | async as status" style="padding: 15px; background: #e2f0d9; border: 1px solid #c6e0b4; border-radius: 5px; color: #385723; margin-top: 20px;">
        {{ status }}
      </div>
    </div>
  `,
  styles: []
})
export class AppComponent {
  private store = inject(Store<{ order: { orderStatus: string, isLoading: boolean } }>);

  customerName: string = '';
  totalAmount: number = 0;

  orderStatus$: Observable<string> = this.store.select(state => state.order.orderStatus);
  isLoading$: Observable<boolean> = this.store.select(state => state.order.isLoading);

  submitOrder() {
    this.store.dispatch(submitOrder({
      customerName: this.customerName,
      totalAmount: this.totalAmount
    }));
  }
}
