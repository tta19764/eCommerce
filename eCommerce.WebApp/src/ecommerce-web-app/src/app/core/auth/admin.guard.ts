import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthStore } from './auth.store';

export const adminGuard: CanActivateFn = () => {
  const auth = inject(AuthStore);

  return auth.isAuthenticated() && auth.isAdmin() ? true : inject(Router).createUrlTree(['/']);
};
