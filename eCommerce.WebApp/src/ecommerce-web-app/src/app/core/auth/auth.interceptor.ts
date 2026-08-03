import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthStore } from './auth.store';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(AuthStore);
  const token = auth.accessToken();
  const isApi = request.url.startsWith(environment.gatewayUrl);
  const isAuth = /\/auth\/(login|register|refresh)$/.test(request.url);
  const isPublicAuthRequest = isAuth || request.url.includes('/auth/confirm-email');

  // Authentication endpoints must not receive a stale access token.
  const outgoing =
    isApi && token && !isPublicAuthRequest
      ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
      : request;
  return next(outgoing).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status !== 401 || isPublicAuthRequest || !auth.refreshToken()) {
        return throwError(() => error);
      }

      // Retry the original request once with the newly issued access token.
      const refresh = auth.refresh();
      if (!refresh) return throwError(() => error);
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
          return throwError(() => refreshError);
        }),
      );
    }),
  );
};
