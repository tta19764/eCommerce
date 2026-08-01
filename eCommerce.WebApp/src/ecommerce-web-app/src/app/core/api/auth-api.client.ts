import { HttpClient } from '@angular/common/http';
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
    return this.http.post<ApiResponse<string>>(`${this.url}/register`, request).pipe(map(apiData));
  }

  refresh(refreshToken: string) {
    return this.http
      .post<ApiResponse<TokenResponse>>(`${this.url}/refresh`, { refreshToken })
      .pipe(map(apiData));
  }
}
