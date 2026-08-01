import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Account } from '../models/account.models';
import { ApiResponse, PagedList, PageQuery } from '../models/api.models';
import { apiData } from './api-base';

@Injectable({ providedIn: 'root' })
export class AccountsApiClient {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.gatewayUrl}/auth-api/v1/auth/accounts`;

  getPage(query: PageQuery = {}) {
    const params = new HttpParams()
      .set('page', query.page ?? 1)
      .set('pageSize', query.pageSize ?? 20);

    return this.http.get<ApiResponse<PagedList<Account>>>(this.url, { params }).pipe(map(apiData));
  }

  delete(accountId: string) {
    return this.http.delete<void>(`${this.url}/${accountId}`);
  }
}
