import { HttpErrorResponse, HttpRequest } from '@angular/common/http';
import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { firstValueFrom, throwError } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { environment } from '../../../environments/environment';
import { authInterceptor } from './auth-interceptor';
import { AuthStore } from './auth-store';

describe('authInterceptor', () => {
  let authStoreMock: Partial<AuthStore>;
  let routerMock: Partial<Router>;
  let refreshTokenSignal = signal<string | null>(null);

  beforeEach(() => {
    refreshTokenSignal = signal<string | null>(null);
    authStoreMock = {
      accessToken: signal('mock-token'),
      refreshToken: refreshTokenSignal,
      logout: vi.fn(),
      refresh: vi.fn(),
    };

    routerMock = {
      navigate: vi.fn(),
    };

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthStore, useValue: authStoreMock },
        { provide: Router, useValue: routerMock },
      ],
    });
  });

  it('triggers logout and redirects to /login on 401 when no refresh token exists', async () => {
    const req = new HttpRequest('GET', `${environment.gatewayUrl}/api/seller/orders`);
    const errorResponse = new HttpErrorResponse({ status: 401, statusText: 'Unauthorized' });

    await expect(
      TestBed.runInInjectionContext(() =>
        firstValueFrom(authInterceptor(req, () => throwError(() => errorResponse))),
      ),
    ).rejects.toBe(errorResponse);

    expect(authStoreMock.logout).toHaveBeenCalled();
    expect(routerMock.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('triggers logout and redirects to /login when refresh attempt fails', async () => {
    refreshTokenSignal.set('invalid-refresh-token');
    (authStoreMock.refresh as any).mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 401, statusText: 'Invalid Refresh' })),
    );

    const req = new HttpRequest('GET', `${environment.gatewayUrl}/api/admin/users`);
    const errorResponse = new HttpErrorResponse({ status: 401, statusText: 'Unauthorized' });

    await expect(
      TestBed.runInInjectionContext(() =>
        firstValueFrom(authInterceptor(req, () => throwError(() => errorResponse))),
      ),
    ).rejects.toSatisfy((err: any) => err instanceof HttpErrorResponse);

    expect(authStoreMock.logout).toHaveBeenCalled();
    expect(routerMock.navigate).toHaveBeenCalledWith(['/login']);
  });
});
