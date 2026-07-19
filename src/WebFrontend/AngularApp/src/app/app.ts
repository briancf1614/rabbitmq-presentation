import { Component, inject } from '@angular/core';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [HttpClientModule, CommonModule, FormsModule],
  template: `
    <div style="padding: 20px; font-family: Arial, sans-serif;">
      <h1>Enterprise Microservices Dashboard</h1>

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
        <button (click)="submitOrder()" style="padding: 10px 15px; background: #007bff; color: white; border: none; border-radius: 3px; cursor: pointer;">Place Order</button>
      </div>

      <div *ngIf="orderStatus" style="padding: 15px; background: #e2f0d9; border: 1px solid #c6e0b4; border-radius: 5px; color: #385723;">
        {{ orderStatus }}
      </div>
    </div>
  `,
  styles: []
})
export class AppComponent {
  private http = inject(HttpClient);

  customerName: string = '';
  totalAmount: number = 0;
  orderStatus: string = '';

  submitOrder() {
    const payload = {
      customerName: this.customerName,
      totalAmount: this.totalAmount
    };

    // The API Gateway will route this to OrderService
    this.http.post('http://localhost:5000/api/orders', payload).subscribe({
      next: (res: any) => {
        this.orderStatus = `Success! Order ID: ${res.orderId} created and event published.`;
      },
      error: (err) => {
        this.orderStatus = `Error creating order: ${err.message}`;
      }
    });
  }
}
