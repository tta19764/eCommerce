import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PagedList, PageQuery } from '../models/api-model';
import {
  Product,
  ProductCategory,
  CreateCategoryRequest,
  CreateProductRequest,
  ProductReview,
  ProductSearchQuery,
  ProductTypeOption,
  UpdateProductRequest,
} from '../models/product-model';
import { apiData } from './api-base';

@Injectable({ providedIn: 'root' })
export class ProductsApiClient {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.gatewayUrl}/product-api/v1/products`;

  getPage(query: ProductSearchQuery = {}) {
    let params = new HttpParams()
      .set('page', query.page ?? 1)
      .set('pageSize', query.pageSize ?? 12);

    params = this.setOptionalParams(params, query);

    return this.http.get<ApiResponse<PagedList<Product>>>(this.url, { params }).pipe(map(apiData));
  }

  getCategories() {
    return this.http
      .get<ApiResponse<ProductCategory[]>>(`${this.url}/categories`)
      .pipe(map(apiData));
  }

  createCategory(request: CreateCategoryRequest) {
    return this.http
      .post<ApiResponse<string>>(`${this.url}/categories`, request)
      .pipe(map(apiData));
  }

  getTypes() {
    return this.http.get<ApiResponse<ProductTypeOption[]>>(`${this.url}/types`).pipe(map(apiData));
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

  createReview(productId: string, request: { rating: number; comment: string }) {
    return this.http
      .post<ApiResponse<ProductReview>>(`${this.url}/${productId}/reviews`, request)
      .pipe(map(apiData));
  }

  deleteReview(productId: string, reviewId: string) {
    return this.http.delete<void>(`${this.url}/${productId}/reviews/${reviewId}`);
  }

  create(request: CreateProductRequest) {
    return this.http.post<ApiResponse<string>>(this.url, request).pipe(map(apiData));
  }

  update(id: string, request: UpdateProductRequest) {
    // Admin mutations should be followed by a refetch because product pages are cached.
    return this.http.put<void>(`${this.url}/${id}`, request);
  }

  delete(id: string) {
    return this.http.delete<void>(`${this.url}/${id}`);
  }

  private setOptionalParams(params: HttpParams, query: ProductSearchQuery): HttpParams {
    const entries: Record<string, string | number | boolean | null | undefined> = {
      query: query.query,
      categoryId: query.categoryId,
      includeSubcategories: query.includeSubcategories,
      productType: query.productType,
      sellerId: query.sellerId,
      minPrice: query.minPrice,
      maxPrice: query.maxPrice,
      minRating: query.minRating,
      inStock: query.inStock,
      sortBy: query.sortBy,
      sortDescending: query.sortDescending,
    };

    return Object.entries(entries).reduce((current, [key, value]) => {
      if (value === null || value === undefined || value === '') {
        return current;
      }

      return current.set(key, value);
    }, params);
  }
}
