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
  private readonly auth = inject(AuthStore);
  private subscription = new Subscription();

  conversationId = input.required<string>();
  close = output<void>();

  @ViewChild('scrollContainer') private scrollContainer!: ElementRef;

  protected readonly messages = signal<ConversationMessage[]>([]);
  protected readonly loading = signal(true);
  protected readonly sending = signal(false);
  protected readonly currentUserId = signal(this.auth.user()?.id);
  protected newMessage = '';

  constructor() {
    effect(() => {
      this.messages();
      setTimeout(() => this.scrollToBottom(), 50);
    });
  }

  ngOnInit() {
    this.loadMessages();
    this.subscribeToRealtime();
    this.markAsRead();
  }

  ngOnDestroy() {
    this.subscription.unsubscribe();
  }

  private loadMessages() {
    this.loading.set(true);
    this.messagingApi.getMessages(this.conversationId()).subscribe({
      next: (result) => {
        this.messages.set([...result.items].reverse());
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
    this.messagingApi.sendMessage(this.conversationId(), { body }).subscribe({
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
