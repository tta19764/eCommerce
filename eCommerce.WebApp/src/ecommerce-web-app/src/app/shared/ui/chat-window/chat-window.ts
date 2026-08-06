import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  inject,
  input,
  OnInit,
  output,
  signal,
  ViewChild,
  effect,
  OnDestroy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MessagingApiClient } from '../../../core/api/messaging-api';
import { MessagingService } from '../../../core/api/messaging-service';
import { UsersApiClient } from '../../../core/api/users-api';
import { AuthStore } from '../../../core/auth/auth-store';
import { ConversationMessage } from '../../../core/models/messaging-model';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-chat-window',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chat-window.html',
  styleUrl: './chat-window.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChatWindow implements OnInit, OnDestroy {
  private readonly messagingApi = inject(MessagingApiClient);
  protected readonly messagingService = inject(MessagingService);
  private readonly usersApi = inject(UsersApiClient);
  private readonly auth = inject(AuthStore);
  private subscription = new Subscription();

  conversationId = input.required<string>();
  close = output<void>();

  @ViewChild('scrollContainer') private scrollContainer!: ElementRef;

  protected readonly messages = signal<ConversationMessage[]>([]);
  protected readonly loading = signal(true);
  protected readonly sending = signal(false);
  protected readonly applicationUserId = signal<string | null>(null);
  protected newMessage = '';

  constructor() {
    effect(() => {
      this.messages();
      setTimeout(() => this.scrollToBottom(), 50);
    });
  }

  protected isSentByCurrentUser(msg: ConversationMessage): boolean {
    if (!msg.senderUserId) return false;
    const s = String(msg.senderUserId).toLowerCase().trim();

    const appUser = this.applicationUserId()?.toLowerCase().trim();
    if (appUser && s === appUser) return true;

    const u = this.auth.user();
    if (!u) return false;

    return (
      (!!u.id && s === u.id.toLowerCase().trim()) ||
      (!!u.userId && s === u.userId.toLowerCase().trim()) ||
      (!!u.email && s === u.email.toLowerCase().trim())
    );
  }

  ngOnInit() {
    this.loadMessages();
    this.subscribeToRealtime();
    this.markAsRead();
    this.loadApplicationUserId();
  }

  ngOnDestroy() {
    this.subscription.unsubscribe();
  }

  private loadApplicationUserId() {
    this.usersApi.getOwn().subscribe({
      next: (profile) => this.applicationUserId.set(profile.id),
      error: () => {},
    });
  }

  private loadMessages() {
    this.loading.set(true);
    this.messagingApi.getMessages(this.conversationId()).subscribe({
      next: (result) => {
        // Backend returns messages in chronological order (CreatedAtUtc ascending). Do not reverse.
        this.messages.set(result.items);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  private subscribeToRealtime() {
    this.subscription.add(
      this.messagingService.messageSent$.subscribe((event) => {
        if (event.conversationId === this.conversationId()) {
          if (!this.messages().some((m) => m.id === event.message.id)) {
            this.messages.update((msgs) => [...msgs, event.message]);
            this.markAsRead();
          }
        }
      })
    );
  }

  private markAsRead() {
    this.messagingApi.markAsRead(this.conversationId()).subscribe();
  }

  protected sendMessage() {
    const body = this.newMessage.trim();
    if (!body || this.sending()) return;

    this.sending.set(true);
    const text = body;
    this.newMessage = '';

    this.messagingApi.sendMessage(this.conversationId(), { body: text }).subscribe({
      next: (messageId) => {
        this.sending.set(false);
        const senderId = this.applicationUserId() ?? this.auth.user()?.id ?? '';
        const newMsg: ConversationMessage = {
          id: messageId || `msg-${Date.now()}`,
          conversationId: this.conversationId(),
          senderUserId: senderId,
          body: text,
          type: 'Text',
          createdAtUtc: new Date().toISOString(),
        };

        if (!this.messages().some((m) => m.id === newMsg.id)) {
          this.messages.update((msgs) => [...msgs, newMsg]);
        }
      },
      error: () => {
        this.sending.set(false);
        this.newMessage = text;
      },
    });
  }

  private scrollToBottom() {
    if (this.scrollContainer) {
      this.scrollContainer.nativeElement.scrollTop = this.scrollContainer.nativeElement.scrollHeight;
    }
  }
}
