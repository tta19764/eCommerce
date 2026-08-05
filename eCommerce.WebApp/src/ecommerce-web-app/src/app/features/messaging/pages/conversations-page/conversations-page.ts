import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  inject,
  OnDestroy,
  OnInit,
  signal,
  ViewChild,
  effect,
} from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { MessagingApiClient } from '../../../../core/api/messaging-api.client';
import { MessagingService } from '../../../../core/api/messaging.service';
import { AuthStore } from '../../../../core/auth/auth.store';
import { Conversation, ConversationMessage } from '../../../../core/models/messaging.models';

@Component({
  selector: 'app-conversations-page',
  standalone: true,
  imports: [CommonModule, FormsModule, DatePipe],
  templateUrl: './conversations-page.html',
  styleUrl: './conversations-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConversationsPage implements OnInit, OnDestroy {
  private readonly messagingApi = inject(MessagingApiClient);
  protected readonly messagingService = inject(MessagingService);
  private readonly auth = inject(AuthStore);
  private subscription = new Subscription();

  @ViewChild('scrollContainer') private scrollContainer!: ElementRef;

  protected readonly conversations = signal<Conversation[]>([]);
  protected readonly activeConversation = signal<Conversation | null>(null);
  protected readonly messages = signal<ConversationMessage[]>([]);
  protected readonly loadingConversations = signal(true);
  protected readonly loadingMessages = signal(false);
  protected readonly sending = signal(false);
  protected readonly currentUserId = signal(this.auth.user()?.id);
  protected searchQuery = '';
  protected newMessage = '';

  constructor() {
    effect(() => {
      this.messages();
      setTimeout(() => this.scrollToBottom(), 50);
    });
  }

  ngOnInit() {
    this.loadConversations();
    this.subscribeToRealtime();
  }

  ngOnDestroy() {
    this.subscription.unsubscribe();
  }

  protected get filteredConversations(): Conversation[] {
    const q = this.searchQuery.toLowerCase().trim();
    if (!q) return this.conversations();
    return this.conversations().filter(
      (c) =>
        c.id.toLowerCase().includes(q) ||
        (c.type && c.type.toLowerCase().includes(q)) ||
        (c.sellerOrderId && c.sellerOrderId.toLowerCase().includes(q)) ||
        (c.productId && c.productId.toLowerCase().includes(q))
    );
  }

  protected selectConversation(conv: Conversation) {
    this.activeConversation.set(conv);
    this.loadMessages(conv.id);
    this.markAsRead(conv.id);
  }

  private loadConversations() {
    this.loadingConversations.set(true);
    this.messagingApi.getConversations({ pageSize: 50 }).subscribe({
      next: (result) => {
        this.conversations.set(result.items);
        this.loadingConversations.set(false);
        if (result.items.length > 0 && !this.activeConversation()) {
          this.selectConversation(result.items[0]);
        }
      },
      error: () => this.loadingConversations.set(false),
    });
  }

  private loadMessages(conversationId: string) {
    this.loadingMessages.set(true);
    this.messagingApi.getMessages(conversationId, { pageSize: 100 }).subscribe({
      next: (result) => {
        this.messages.set([...result.items].reverse());
        this.loadingMessages.set(false);
      },
      error: () => this.loadingMessages.set(false),
    });
  }

  private subscribeToRealtime() {
    this.subscription.add(
      this.messagingService.messageSent$.subscribe((event) => {
        this.messagingApi.getConversations({ pageSize: 50 }).subscribe((result) => {
          this.conversations.set(result.items);
        });

        if (this.activeConversation()?.id === event.conversationId) {
          if (!this.messages().some((m) => m.id === event.message.id)) {
            this.messages.update((msgs) => [...msgs, event.message]);
            this.markAsRead(event.conversationId);
          }
        }
      })
    );

    this.subscription.add(
      this.messagingService.conversationCreated$.subscribe(() => {
        this.loadConversations();
      })
    );
  }

  private markAsRead(conversationId: string) {
    this.messagingApi.markAsRead(conversationId).subscribe();
  }

  protected sendMessage() {
    const active = this.activeConversation();
    const body = this.newMessage.trim();
    if (!active || !body || this.sending()) return;

    this.sending.set(true);
    this.messagingApi.sendMessage(active.id, { body }).subscribe({
      next: () => {
        this.newMessage = '';
        this.sending.set(false);
      },
      error: () => this.sending.set(false),
    });
  }

  private scrollToBottom() {
    if (this.scrollContainer) {
      this.scrollContainer.nativeElement.scrollTop = this.scrollContainer.nativeElement.scrollHeight;
    }
  }
}
