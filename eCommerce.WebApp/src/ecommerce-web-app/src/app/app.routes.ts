import { Routes } from '@angular/router';
import { adminGuard } from './core/auth/admin.guard';
import { authGuard } from './core/auth/auth.guard';
import { sellerGuard } from './core/auth/seller.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./features/catalog/pages/catalog-page/catalog-page').then((m) => m.CatalogPage),
    title: 'Shop · eCommerce',
  },
  {
    path: 'products/:id',
    loadComponent: () =>
      import('./features/catalog/pages/product-page/product-page').then((m) => m.ProductPage),
    title: 'Product · eCommerce',
  },
  {
    path: 'cart',
    loadComponent: () =>
      import('./features/cart/pages/cart-page/cart-page').then((m) => m.CartPage),
    title: 'Your bag · eCommerce',
  },
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/pages/login-page/login-page').then((m) => m.LoginPage),
    title: 'Sign in · eCommerce',
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./features/auth/pages/register-page/register-page').then((m) => m.RegisterPage),
    title: 'Create account · eCommerce',
  },
  {
    path: 'orders',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/orders/pages/orders-page/orders-page').then((m) => m.OrdersPage),
    title: 'Orders · eCommerce',
  },
  {
    path: 'messages',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/messaging/pages/conversations-page/conversations-page').then(
        (m) => m.ConversationsPage,
      ),
    title: 'Messages · eCommerce',
  },
  {
    path: 'profile',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/profile/pages/profile-page/profile-page').then((m) => m.ProfilePage),
    title: 'My profile · eCommerce',
  },
  {
    path: 'confirm-email',
    loadComponent: () =>
      import('./features/auth/pages/confirm-email-page/confirm-email-page').then(
        (m) => m.ConfirmEmailPage,
      ),
    title: 'Confirm email · eCommerce',
  },
  {
    path: 'seller',
    canActivate: [sellerGuard],
    loadComponent: () =>
      import('./features/seller/pages/seller-products-page/seller-products-page').then(
        (m) => m.SellerProductsPage,
      ),
    title: 'Seller portal · eCommerce',
  },
  {
    path: 'admin',
    canActivate: [adminGuard],
    loadComponent: () =>
      import('./features/admin/layout/admin-layout/admin-layout').then((m) => m.AdminLayout),
    children: [
      {
        path: '',
        pathMatch: 'full',
        redirectTo: 'products',
      },
      {
        path: 'products',
        loadComponent: () =>
          import('./features/admin/pages/admin-products-page/admin-products-page').then(
            (m) => m.AdminProductsPage,
          ),
        title: 'Manage products · eCommerce',
      },
      {
        path: 'categories',
        loadComponent: () =>
          import('./features/admin/pages/admin-categories-page/admin-categories-page').then(
            (m) => m.AdminCategoriesPage,
          ),
        title: 'Category Editor · eCommerce',
      },
      {
        path: 'users',
        loadComponent: () =>
          import('./features/admin/pages/admin-users-page/admin-users-page').then(
            (m) => m.AdminUsersPage,
          ),
        title: 'Manage users · eCommerce',
      },
    ],
  },
  {
    path: '**',
    loadComponent: () =>
      import('./shared/ui/not-found-page/not-found-page').then((m) => m.NotFoundPage),
    title: 'Page not found · eCommerce',
  },
];
