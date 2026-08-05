import { CurrencyPipe, DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { OrdersApiClient } from '../../../../core/api/orders-api';
import { MessagingApiClient } from '../../../../core/api/messaging-api';
import { Order } from '../../../../core/models/order-model';
import { ChatWindow } from '../../../../shared/ui/chat-window/chat-window';

@Component({
  selector: 'app-orders-page',
  imports: [CurrencyPipe, DatePipe, RouterLink, ChatWindow],
  templateUrl: './orders-page.html',
  styleUrl: './orders-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrdersPage {
  private readonly api = inject(OrdersApiClient);
  private readonly messagingApi = inject(MessagingApiClient);

  protected readonly orders = signal<Order[]>([]);
  protected readonly loading = signal(true);
  protected readonly failed = signal(false);
  protected readonly activeConversationId = signal<string | null>(null);

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

  protected openChat(sellerOrderId: string): void {
    this.messagingApi.getConversations().subscribe((conversations) => {
      const existing = conversations.items.find((c) => c.sellerOrderId === sellerOrderId);
      if (existing) {
        this.activeConversationId.set(existing.id);
      } else {
        // Start a new conversation for this order
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
