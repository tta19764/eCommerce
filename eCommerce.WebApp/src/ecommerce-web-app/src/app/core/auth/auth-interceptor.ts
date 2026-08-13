import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthStore } from './auth-store';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(AuthStore);
  const router = inject(Router);
  const token = auth.accessToken();
  const isApi = request.url.startsWith(environment.gatewayUrl);
  const isAuth = /\/auth\/(login|register(?:\/seller)?|refresh)$/.test(request.url);
  const isPublicAuthRequest = isAuth || request.url.includes('/auth/confirm-email');

  // Authentication endpoints must not receive a stale access token.
  const outgoing =
    isApi && token && !isPublicAuthRequest
      ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
      : request;
  return next(outgoing).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status !== 401 || isPublicAuthRequest) {
        return throwError(() => error);
      }

      if (!auth.refreshToken()) {
        auth.logout();
        router.navigate(['/login']);
        return throwError(() => error);
      }

      // Retry the original request once with the newly issued access token.
      const refresh = auth.refresh();
      if (!refresh) {
        auth.logout();
        router.navigate(['/login']);
        return throwError(() => error);
      }

      return refresh.pipe(
        switchMap((tokens) =>
          next(
            request.clone({
              setHeaders: { Authorization: `Bearer ${tokens.accessToken}` },
            }),
          ),
        ),
        catchError((refreshError) => {
          auth.logout();
          router.navigate(['/login']);
          return throwError(() => refreshError);
        }),
      );
    }),
  );
};
