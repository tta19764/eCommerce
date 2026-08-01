import { CurrencyPipe, DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { OrdersApiClient } from '../../../../core/api/orders-api.client';
import { Order } from '../../../../core/models/order.models';

@Component({
  selector: 'app-orders-page',
  imports: [CurrencyPipe, DatePipe, RouterLink],
  templateUrl: './orders-page.html',
  styleUrl: './orders-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrdersPage {
  private readonly api = inject(OrdersApiClient);
  protected readonly orders = signal<Order[]>([]);
  protected readonly loading = signal(true);
  protected readonly failed = signal(false);
  constructor() {
    this.api.getOwn().subscribe({
      next: (result) => {
        this.orders.set(result.items);
        this.loading.set(false);
      },
      error: () => {
        this.failed.set(true);
        this.loading.set(false);
      },
    });
  }
}
