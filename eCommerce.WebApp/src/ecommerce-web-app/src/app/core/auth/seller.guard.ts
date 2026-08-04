import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthStore } from './auth.store';

export const sellerGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthStore);

  if (!auth.isAuthenticated()) {
    return inject(Router).createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
  }

  return auth.isSeller() || auth.isAdmin() ? true : inject(Router).createUrlTree(['/']);
};
