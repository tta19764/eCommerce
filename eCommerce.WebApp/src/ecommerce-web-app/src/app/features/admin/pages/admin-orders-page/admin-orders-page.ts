import { AppCurrencyPipe } from '../../../../shared/pipes/app-currency.pipe';
import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { OrdersApiClient } from '../../../../core/api/orders-api';
import { apiErrorMessage } from '../../../../core/api/api-base';
import { Order, OrderStatus } from '../../../../core/models/order-model';

@Component({
  selector: 'app-admin-orders-page',
  standalone: true,
  imports: [AppCurrencyPipe, DatePipe, FormsModule],
  templateUrl: './admin-orders-page.html',
  styleUrl: './admin-orders-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminOrdersPage {
  private readonly ordersApi = inject(OrdersApiClient);

  protected readonly orders = signal<Order[]>([]);
  protected readonly totalCount = signal(0);
  protected readonly page = signal(1);
  protected readonly pageSize = signal(10);

  // Filters & Sorting
  protected readonly minPriceInput = signal<number | null>(null);
  protected readonly maxPriceInput = signal<number | null>(null);
  protected readonly sortByOrderPrice = signal(false);
  protected readonly sortDescending = signal(true);

  // UI Loaders & Feedback
  protected readonly loading = signal(true);
  protected readonly updatingId = signal<string | null>(null);
  protected readonly error = signal('');
  protected readonly success = signal('');

  constructor() {
    this.loadOrders();
  }

  protected totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount() / this.pageSize()));
  }

  protected setPage(newPage: number): void {
    if (newPage < 1 || newPage > this.totalPages()) return;
    this.page.set(newPage);
    this.loadOrders();
  }

  protected applyFilters(): void {
    this.page.set(1);
    this.loadOrders();
  }

  protected resetFilters(): void {
    this.minPriceInput.set(null);
    this.maxPriceInput.set(null);
    this.sortByOrderPrice.set(false);
    this.sortDescending.set(true);
    this.page.set(1);
    this.loadOrders();
  }

  protected updateOrderStatus(orderId: string, status: OrderStatus): void {
    this.updatingId.set(orderId);
    this.clearMessages();

    this.ordersApi.updateStatus(orderId, status).subscribe({
      next: () => {
        this.success.set(`Global order #${orderId.slice(0, 8)} status updated to ${status}.`);
        this.updatingId.set(null);
        this.loadOrders();
      },
      error: (err) => {
        console.error('[Admin updateOrderStatus error]:', err);
        this.error.set(apiErrorMessage(err));
        this.updatingId.set(null);
      },
    });
  }

  protected updateSellerOrderStatus(sellerOrderId: string, status: OrderStatus): void {
    this.updatingId.set(sellerOrderId);
    this.clearMessages();

    this.ordersApi.updateSellerOrderStatus(sellerOrderId, status).subscribe({
      next: () => {
        this.success.set(`Seller order #${sellerOrderId.slice(0, 8)} status updated to ${status}.`);
        this.updatingId.set(null);
        this.loadOrders();
      },
      error: (err) => {
        console.error('[Admin updateSellerOrderStatus error]:', err);
        this.error.set(apiErrorMessage(err));
        this.updatingId.set(null);
      },
    });
  }

  protected loadOrders(): void {
    this.loading.set(true);
    this.clearMessages();

    this.ordersApi
      .getPage({
        page: this.page(),
        pageSize: this.pageSize(),
        minOrderPrice: this.minPriceInput(),
        maxOrderPrice: this.maxPriceInput(),
        sortByOrderPrice: this.sortByOrderPrice(),
        sortDescending: this.sortDescending(),
      })
      .subscribe({
        next: (res) => {
          this.orders.set(res.items);
          this.totalCount.set(res.totalCount);
          this.loading.set(false);
        },
        error: (err) => {
          console.error('[Admin loadOrders error]:', err);
          this.error.set(apiErrorMessage(err));
          this.loading.set(false);
        },
      });
  }

  private clearMessages(): void {
    this.error.set('');
    this.success.set('');
  }
}
