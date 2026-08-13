import { TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { AuthApiClient } from '../api/auth-api';
import { TokenResponse } from '../models/auth-model';
import { AuthStore } from './auth-store';

describe('AuthStore', () => {
  let store: AuthStore;
  let authApiMock: Partial<AuthApiClient>;

  function buildToken(role: string, expiresAtUtc: string): TokenResponse {
    const header = btoa(JSON.stringify({ alg: 'HS256', typ: 'JWT' }));
    const payload = btoa(
      JSON.stringify({
        sub: 'user-123',
        email: 'user@example.com',
        roles: [role],
      }),
    );
    const accessToken = `${header}.${payload}.signature`;

    return {
      accessToken,
      refreshToken: 'refresh-token-123',
      expiresAtUtc,
      refreshExpiresAtUtc: expiresAtUtc,
    };
  }

  beforeEach(() => {
    sessionStorage.clear();
    authApiMock = {};

    TestBed.configureTestingModule({
      providers: [AuthStore, { provide: AuthApiClient, useValue: authApiMock }],
    });

    store = TestBed.inject(AuthStore);
  });

  it('evaluates isAdmin and isSeller as true for valid active token', () => {
    const futureDate = new Date(Date.now() + 3600 * 1000).toISOString();
    const adminToken = buildToken('Admin', futureDate);

    (store as any).setTokens(adminToken);

    expect(store.isAuthenticated()).toBe(true);
    expect(store.isAdmin()).toBe(true);
    expect(store.isSeller()).toBe(false);
  });

  it('evaluates isAdmin and isSeller as false when token is expired', () => {
    const pastDate = new Date(Date.now() - 3600 * 1000).toISOString();
    const expiredAdminToken = buildToken('Admin', pastDate);

    (store as any).setTokens(expiredAdminToken);

    expect(store.isAuthenticated()).toBe(false);
    expect(store.isAdmin()).toBe(false);
    expect(store.isSeller()).toBe(false);
  });

  it('clears expired tokens from sessionStorage on restore', () => {
    const pastDate = new Date(Date.now() - 3600 * 1000).toISOString();
    const expiredAdminToken = buildToken('Admin', pastDate);

    sessionStorage.setItem('ecommerce.session', JSON.stringify(expiredAdminToken));

    const restored = (store as any).restore();

    expect(restored).toBeNull();
    expect(sessionStorage.getItem('ecommerce.session')).toBeNull();
  });

  it('resets isAdmin and isSeller when logout is invoked', () => {
    const futureDate = new Date(Date.now() + 3600 * 1000).toISOString();
    const sellerToken = buildToken('Seller', futureDate);

    (store as any).setTokens(sellerToken);
    expect(store.isSeller()).toBe(true);

    store.logout();

    expect(store.isAuthenticated()).toBe(false);
    expect(store.isAdmin()).toBe(false);
    expect(store.isSeller()).toBe(false);
  });
});
