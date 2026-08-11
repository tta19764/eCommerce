import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-model';
import { CreatePaymentResponse, Payment } from '../models/payment-model';
import { apiData } from './api-base';

@Injectable({ providedIn: 'root' })
export class PaymentsApiClient {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.gatewayUrl}/payment-api/v1`;

  /** Returns the public Stripe key; secret and webhook keys never cross this boundary. */
  getConfig() {
    return this.http.get<{ publishableKey: string }>(`${this.url}/payments/config`);
  }

  /** Creates or reuses the authenticated owner's intent using the order's frozen backend total. */
  create(orderId: string) {
    return this.http
      .post<ApiResponse<CreatePaymentResponse>>(`${this.url}/payments`, { orderId })
      .pipe(map(apiData));
  }

  /** Reads the internal payment projection after enforcing customer ownership server-side. */
  get(paymentId: string) {
    return this.http
      .get<ApiResponse<Payment>>(`${this.url}/payments/${paymentId}`)
      .pipe(map(apiData));
  }
}
