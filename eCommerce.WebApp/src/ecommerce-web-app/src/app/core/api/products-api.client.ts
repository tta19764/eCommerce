import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PagedList, PageQuery } from '../models/api.models';
import { Product, ProductReview, ProductUpsertRequest } from '../models/product.models';
import { apiData } from './api-base';

@Injectable({ providedIn: 'root' })
export class ProductsApiClient {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.gatewayUrl}/product-api/v1/products`;

  getPage(query: PageQuery = {}) {
    const params = new HttpParams()
      .set('page', query.page ?? 1)
      .set('pageSize', query.pageSize ?? 12);

    return this.http.get<ApiResponse<PagedList<Product>>>(this.url, { params }).pipe(map(apiData));
  }

  getById(id: string) {
    return this.http.get<ApiResponse<Product>>(`${this.url}/${id}`).pipe(map(apiData));
  }

  getReviews(productId: string, query: PageQuery = {}) {
    const params = new HttpParams()
      .set('page', query.page ?? 1)
      .set('pageSize', query.pageSize ?? 10);

    return this.http
      .get<ApiResponse<PagedList<ProductReview>>>(`${this.url}/${productId}/reviews`, { params })
      .pipe(map(apiData));
  }

  create(request: ProductUpsertRequest) {
    return this.http.post<ApiResponse<string>>(this.url, request).pipe(map(apiData));
  }

  update(id: string, request: ProductUpsertRequest) {
    // Admin mutations should be followed by a refetch because product pages are cached.
    return this.http.put<void>(`${this.url}/${id}`, request);
  }

  delete(id: string) {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
