import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PagedList, PageQuery } from '../models/api.models';
import { Order, OrderItemRequest } from '../models/order.models';
import { apiData } from './api-base';

@Injectable({ providedIn: 'root' })
export class OrdersApiClient {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.gatewayUrl}/order-api/v1/orders`;

  getOwn(query: PageQuery = {}) {
    const params = new HttpParams()
      .set('page', query.page ?? 1)
      .set('pageSize', query.pageSize ?? 10);

    return this.http
      .get<ApiResponse<PagedList<Order>>>(`${this.url}/own`, { params })
      .pipe(map(apiData));
  }

  create(clientId: string, items: OrderItemRequest[]) {
    return this.http.post<ApiResponse<string>>(this.url, { clientId, items }).pipe(map(apiData));
  }
}
