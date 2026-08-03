import { computed, inject, Injectable, signal } from '@angular/core';
import { tap } from 'rxjs';
import { AuthApiClient } from '../api/auth-api.client';
import { AuthUser, LoginRequest, RegisterRequest, TokenResponse } from '../models/auth.models';

const STORAGE_KEY = 'ecommerce.session';

// Only claims used by the UI are represented here; Keycloak may include many more.
interface AccessTokenPayload {
  sub: string;
  email?: string;
  preferred_username?: string;
  realm_access?: { roles?: string[] };
  roles?: string[];
  user_id?: string;
  userId?: string;
}

@Injectable({ providedIn: 'root' })
export class AuthStore {
  private readonly api = inject(AuthApiClient);
  // Session storage survives refreshes but is cleared when the browser session ends.
  private readonly tokens = signal<TokenResponse | null>(this.restore());

  readonly user = computed(() => this.decodeUser(this.tokens()?.accessToken));
  readonly isAuthenticated = computed(() => !!this.user() && !this.isExpired(this.tokens()));
  readonly isAdmin = computed(() => this.user()?.role === 'Admin');
  readonly accessToken = computed(() => this.tokens()?.accessToken ?? null);
  readonly refreshToken = computed(() => this.tokens()?.refreshToken ?? null);

  login(request: LoginRequest) {
    return this.api.login(request).pipe(tap((value) => this.setTokens(value)));
  }

  register(request: RegisterRequest) {
    return this.api.register(request);
  }

  refresh() {
    const token = this.refreshToken();

    return token ? this.api.refresh(token).pipe(tap((value) => this.setTokens(value))) : null;
  }

  logout(): void {
    this.tokens.set(null);
    sessionStorage.removeItem(STORAGE_KEY);
  }

  private setTokens(tokens: TokenResponse): void {
    this.tokens.set(tokens);
    sessionStorage.setItem(STORAGE_KEY, JSON.stringify(tokens));
  }

  private restore(): TokenResponse | null {
    try {
      const value = sessionStorage.getItem(STORAGE_KEY);
      return value ? (JSON.parse(value) as TokenResponse) : null;
    } catch {
      return null;
    }
  }

  private isExpired(tokens: TokenResponse | null): boolean {
    return !tokens || Date.parse(tokens.expiresAtUtc) <= Date.now();
  }

  private decodeUser(token?: string): AuthUser | null {
    if (!token) {
      return null;
    }

    try {
      // JWT decoding is used for UI state only and is not an authorization decision.
      const payload = JSON.parse(this.decodeBase64Url(token.split('.')[1])) as AccessTokenPayload;
      const roles = payload.realm_access?.roles ?? payload.roles ?? [];

      return {
        id: payload.sub,
        email: payload.email ?? payload.preferred_username ?? '',
        role: this.resolveApplicationRole(roles),
        userId: payload.user_id ?? payload.userId ?? null,
      };
    } catch {
      return null;
    }
  }

  private resolveApplicationRole(roles: string[]): 'Admin' | 'Customer' {
    // Hide Keycloak infrastructure roles by reducing them to one application role.
    return roles.some((role) => role.toLowerCase() === 'admin') ? 'Admin' : 'Customer';
  }

  private decodeBase64Url(value: string): string {
    const base64 = value.replace(/-/g, '+').replace(/_/g, '/');
    return decodeURIComponent(
      atob(base64)
        .split('')
        .map((char) => `%${char.charCodeAt(0).toString(16).padStart(2, '0')}`)
        .join(''),
    );
  }
}
