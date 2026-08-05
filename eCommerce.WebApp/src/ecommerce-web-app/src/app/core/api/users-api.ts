import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-model';
import { UpdateUserProfileRequest, UserProfile } from '../models/user-model';
import { apiData } from './api-base';

@Injectable({ providedIn: 'root' })
export class UsersApiClient {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.gatewayUrl}/user-api/v1/users`;

  getById(userId: string) {
    return this.http.get<ApiResponse<UserProfile>>(`${this.url}/${userId}`).pipe(map(apiData));
  }

  update(userId: string, request: UpdateUserProfileRequest) {
    return this.http.put<void>(`${this.url}/${userId}`, request);
  }

  // Personal profile endpoints resolve the profile ID from authenticated claims.
  getOwn() {
    return this.http.get<ApiResponse<UserProfile>>(`${this.url}/own`).pipe(map(apiData));
  }

  // Never add a profile ID to this request; ownership is enforced by the backend.
  updateOwn(request: UpdateUserProfileRequest) {
    return this.http.put<void>(`${this.url}/own`, request);
  }
}
