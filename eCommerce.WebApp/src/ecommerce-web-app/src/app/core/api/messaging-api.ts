import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PagedList, PageQuery } from '../models/api-model';
import {
  Conversation,
  ConversationMessage,
  SendMessageRequest,
  StartProductInquiryRequest,
  StartSellerOrderConversationRequest,
} from '../models/messaging-model';
import { apiData } from './api-base';

@Injectable({ providedIn: 'root' })
export class MessagingApiClient {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.gatewayUrl}/messaging-api/v1/conversations`;

  getConversations(query: PageQuery = {}) {
    const params = new HttpParams()
      .set('page', query.page ?? 1)
      .set('pageSize', query.pageSize ?? 20);

    return this.http
      .get<ApiResponse<PagedList<Conversation>>>(this.url, { params })
      .pipe(map(apiData));
  }

  getConversation(id: string) {
    return this.http.get<ApiResponse<Conversation>>(`${this.url}/${id}`).pipe(map(apiData));
  }

  getMessages(conversationId: string, query: PageQuery = {}) {
    const params = new HttpParams()
      .set('page', query.page ?? 1)
      .set('pageSize', query.pageSize ?? 50);

    return this.http
      .get<ApiResponse<PagedList<ConversationMessage>>>(`${this.url}/${conversationId}/messages`, {
        params,
      })
      .pipe(map(apiData));
  }

  sendMessage(conversationId: string, request: SendMessageRequest) {
    return this.http
      .post<ApiResponse<string>>(`${this.url}/${conversationId}/messages`, request)
      .pipe(map(apiData));
  }

  markAsRead(conversationId: string) {
    return this.http.post<void>(`${this.url}/${conversationId}/read`, {});
  }

  startProductInquiry(request: StartProductInquiryRequest) {
    return this.http
      .post<ApiResponse<string>>(`${this.url}/product-inquiry`, request)
      .pipe(map(apiData));
  }

  startSellerOrderConversation(request: StartSellerOrderConversationRequest) {
    return this.http
      .post<ApiResponse<string>>(`${this.url}/seller-order-conversation`, request)
      .pipe(map(apiData));
  }
}
