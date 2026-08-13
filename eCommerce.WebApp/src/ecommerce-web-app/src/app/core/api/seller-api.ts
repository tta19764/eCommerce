import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PagedList, PageQuery } from '../models/api-model';
import {
  CreateSellerApplicationRequest,
  CreateStoreReviewRequest,
  PendingSellerApplicationResponse,
  RejectSellerRequest,
  SellerResponse,
  StoreResponse,
  StoreReviewResponse,
} from '../models/seller-model';
import { apiData } from './api-base';

@Injectable({ providedIn: 'root' })
export class SellerApiClient {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.gatewayUrl}/seller-api/v1`;

  createApplication(request: CreateSellerApplicationRequest) {
    return this.http
      .post<ApiResponse<string>>(`${this.baseUrl}/sellers/own/application`, request)
      .pipe(map(apiData));
  }

  getOwnSeller() {
    return this.http
      .get<ApiResponse<SellerResponse>>(`${this.baseUrl}/sellers/own`)
      .pipe(map(apiData));
  }

  getPendingSellers(query: PageQuery = {}) {
    const params = new HttpParams()
      .set('page', query.page ?? 1)
      .set('pageSize', query.pageSize ?? 10);

    return this.http
      .get<ApiResponse<PagedList<PendingSellerApplicationResponse>>>(
        `${this.baseUrl}/sellers/pending`,
        { params },
      )
      .pipe(map(apiData));
  }

  approveSeller(sellerId: string) {
    return this.http.post<void>(`${this.baseUrl}/sellers/${sellerId}/approve`, {});
  }

  rejectSeller(sellerId: string, request: RejectSellerRequest) {
    return this.http.post<void>(`${this.baseUrl}/sellers/${sellerId}/reject`, request);
  }

  getStoreBySlug(slug: string) {
    return this.http
      .get<ApiResponse<StoreResponse>>(`${this.baseUrl}/stores/${slug}`)
      .pipe(map(apiData));
  }

  getStoreReviews(storeId: string, query: PageQuery = {}) {
    const params = new HttpParams()
      .set('page', query.page ?? 1)
      .set('pageSize', query.pageSize ?? 10);

    return this.http
      .get<ApiResponse<StoreReviewResponse[]>>(`${this.baseUrl}/stores/${storeId}/reviews`, {
        params,
      })
      .pipe(map(apiData));
  }

  createStoreReview(storeId: string, request: CreateStoreReviewRequest) {
    return this.http
      .post<ApiResponse<string>>(`${this.baseUrl}/stores/${storeId}/reviews`, request)
      .pipe(map(apiData));
  }
}
