import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api.models';
import { LoginRequest, RegisterRequest, TokenResponse } from '../models/auth.models';
import { apiData } from './api-base';

@Injectable({ providedIn: 'root' })
export class AuthApiClient {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.gatewayUrl}/auth-api/v1/auth`;

  login(request: LoginRequest) {
    return this.http
      .post<ApiResponse<TokenResponse>>(`${this.url}/login`, request)
      .pipe(map(apiData));
  }

  register(request: RegisterRequest) {
    // Public registration always creates a Customer account.
    return this.http.post<ApiResponse<string>>(`${this.url}/register`, request).pipe(map(apiData));
  }

  registerAdmin(request: RegisterRequest) {
    // This protected endpoint is available only to an existing administrator.
    return this.http
      .post<ApiResponse<string>>(`${this.url}/register/admin`, request)
      .pipe(map(apiData));
  }

  confirmEmail(accountId: string, email: string) {
    const params = new HttpParams().set('accountId', accountId).set('email', email);

    // A successful command intentionally returns null data inside the standard envelope.
    return this.http.get<ApiResponse<null>>(`${this.url}/confirm-email`, { params }).pipe(
      map((response) => {
        if (response.error) {
          throw new Error(response.error.name);
        }
      }),
    );
  }

  refresh(refreshToken: string) {
    return this.http
      .post<ApiResponse<TokenResponse>>(`${this.url}/refresh`, { refreshToken })
      .pipe(map(apiData));
  }
}
