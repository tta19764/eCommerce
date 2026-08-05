import { inject, Injectable, OnDestroy, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthStore } from '../auth/auth-store';
import { ConversationMessage } from '../models/messaging-model';
import { MessagingApiClient } from './messaging-api';
import {
  ConversationCreatedRealtimeEvent,
  ConversationReadRealtimeEvent,
  MessageSentRealtimeEvent,
} from '../models/messaging-model';

@Injectable({ providedIn: 'root' })
export class MessagingService implements OnDestroy {
  private readonly authStore = inject(AuthStore);
  private hubConnection: signalR.HubConnection | null = null;

  // Events
  private readonly messageSentSubject = new Subject<MessageSentRealtimeEvent>();
  readonly messageSent$ = this.messageSentSubject.asObservable();

  private readonly conversationCreatedSubject = new Subject<ConversationCreatedRealtimeEvent>();
  readonly conversationCreated$ = this.conversationCreatedSubject.asObservable();

  private readonly conversationReadSubject = new Subject<ConversationReadRealtimeEvent>();
  readonly conversationRead$ = this.conversationReadSubject.asObservable();

  // Connection State
  readonly isConnected = signal(false);

  constructor() {
    // We could automatically connect if the user is authenticated,
    // or provide explicit start/stop methods.
  }

  async startConnection() {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      return;
    }

    const hubUrl = `${environment.gatewayUrl}/messaging-api/hubs/conversations`;

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => this.authStore.accessToken() ?? '',
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('MessageSent', (event: MessageSentRealtimeEvent) => {
      this.messageSentSubject.next(event);
    });

    this.hubConnection.on('ConversationCreated', (event: ConversationCreatedRealtimeEvent) => {
      this.conversationCreatedSubject.next(event);
    });

    this.hubConnection.on('ConversationRead', (event: ConversationReadRealtimeEvent) => {
      this.conversationReadSubject.next(event);
    });

    try {
      await this.hubConnection.start();
      this.isConnected.set(true);
      console.log('SignalR: Connected to Conversations Hub');
    } catch (err) {
      console.error('SignalR: Error while starting connection: ' + err);
      this.isConnected.set(false);
    }

    this.hubConnection.onclose(() => {
      this.isConnected.set(false);
    });
  }

  async stopConnection() {
    if (this.hubConnection) {
      await this.hubConnection.stop();
      this.hubConnection = null;
      this.isConnected.set(false);
    }
  }

  ngOnDestroy() {
    this.stopConnection();
  }
}
