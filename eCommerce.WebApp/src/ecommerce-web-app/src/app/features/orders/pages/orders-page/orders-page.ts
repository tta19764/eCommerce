import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnDestroy, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MessagingApiClient } from '../../../../core/api/messaging-api';
import { OrdersApiClient } from '../../../../core/api/orders-api';
import { Order } from '../../../../core/models/order-model';
import { PaymentStateService } from '../../../../core/services/payment-state.service';
import { AppCurrencyPipe } from '../../../../shared/pipes/app-currency.pipe';
import { ChatWindow } from '../../../../shared/ui/chat-window/chat-window';

@Component({
  selector: 'app-orders-page',
  imports: [AppCurrencyPipe, DatePipe, RouterLink, ChatWindow],
  templateUrl: './orders-page.html',
  styleUrl: './orders-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrdersPage implements OnDestroy {
  private readonly api = inject(OrdersApiClient);
  private readonly messagingApi = inject(MessagingApiClient);
  private readonly paymentState = inject(PaymentStateService);
  private readonly route = inject(ActivatedRoute);
  private pollTimeout: ReturnType<typeof setTimeout> | null = null;
  private pollCount = 0;

  protected readonly orders = signal<Order[]>([]);
  protected readonly loading = signal(true);
  protected readonly failed = signal(false);
  protected readonly activeConversationId = signal<string | null>(null);

  constructor() {
    this.fetchOrders();
  }

  ngOnDestroy(): void {
    if (this.pollTimeout) {
      clearTimeout(this.pollTimeout);
    }
  }

  protected fetchOrders(): void {
    this.api.getOwn().subscribe({
      next: (result) => {
        this.orders.set(result.items);
        this.paymentState.reconcile(result.items);
        this.loading.set(false);

        // If any order is marked as recently paid but still Confirmed in DB, poll briefly for DB sync
        const hasPendingSync = result.items.some(
          (o) => o.status === 'Confirmed' && this.paymentState.isRecentlyPaid(o.id)
        );

        if (hasPendingSync && this.pollCount < 4) {
          this.pollCount++;
          this.pollTimeout = setTimeout(() => this.fetchOrders(), 3000);
        }
      },
      error: () => {
        this.failed.set(true);
        this.loading.set(false);
      },
    });
  }

  protected isRecentlyPaid(orderId: string): boolean {
    return this.paymentState.isRecentlyPaid(orderId);
  }

  protected isPayable(order: Order): boolean {
    return order.status === 'Confirmed' && !order.paidOnUtc && !this.isRecentlyPaid(order.id);
  }

  protected openChat(sellerOrderId: string): void {
    this.messagingApi.getConversations().subscribe((conversations) => {
      const existing = conversations.items.find((c) => c.sellerOrderId === sellerOrderId);
      if (existing) {
        this.activeConversationId.set(existing.id);
      } else {
        this.messagingApi
          .startSellerOrderConversation({
            sellerOrderId: sellerOrderId,
            initialMessage: 'I have a question about my order.',
          })
          .subscribe((id) => this.activeConversationId.set(id));
      }
    });
  }
}
