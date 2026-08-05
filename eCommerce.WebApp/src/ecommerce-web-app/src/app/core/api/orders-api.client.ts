import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PagedList, PageQuery } from '../models/api.models';
import {
  Order,
  OrderItemRequest,
  OrderStatus,
  SellerOrder,
  UpdateOrderStatusRequest,
} from '../models/order.models';
import { apiData } from './api-base';

@Injectable({ providedIn: 'root' })
export class OrdersApiClient {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.gatewayUrl}/order-api/v1/orders`;

  // The backend resolves the owner from the access-token claims.
  getOwn(query: PageQuery = {}) {
    const params = new HttpParams()
      .set('page', query.page ?? 1)
      .set('pageSize', query.pageSize ?? 10);

    return this.http
      .get<ApiResponse<PagedList<Order>>>(`${this.url}/own`, { params })
      .pipe(map(apiData));
  }

  // Reserved for admin/backend workflows that intentionally select a client.
  createForClient(clientId: string, items: OrderItemRequest[]) {
    return this.http.post<ApiResponse<string>>(this.url, { clientId, items }).pipe(map(apiData));
  }

  // Customer checkout must never send a browser-supplied client identifier.
  createOwn(items: OrderItemRequest[]) {
    return this.http.post<ApiResponse<string>>(`${this.url}/own`, { items }).pipe(map(apiData));
  }

  // Admin workflow: drives status transitions such as Confirmed, Paid, Shipped, Completed, or Cancelled.
  updateStatus(orderId: string, status: OrderStatus) {
    const request: UpdateOrderStatusRequest = { status };
    return this.http.patch<void>(`${this.url}/${orderId}/status`, request);
  }

  // Customer workflow: cancels only the current user's own order.
  cancelOwn(orderId: string) {
    return this.http.post<void>(`${this.url}/${orderId}/cancel`, {});
  }

  // Seller workflow: gets orders belonging to the authenticated seller.
  getSellerOrders(query: PageQuery = {}) {
    const params = new HttpParams()
      .set('page', query.page ?? 1)
      .set('pageSize', query.pageSize ?? 10);

    return this.http
      .get<ApiResponse<PagedList<SellerOrder>>>(`${this.url}/seller`, { params })
      .pipe(map(apiData));
  }

  // Seller workflow: updates status for a specific seller order group.
  updateSellerOrderStatus(sellerOrderId: string, status: OrderStatus) {
    const request: UpdateOrderStatusRequest = { status };
    return this.http.patch<void>(`${this.url}/seller/${sellerOrderId}/status`, request);
  }
}
