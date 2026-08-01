import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { OrdersApiClient } from '../../../../core/api/orders-api.client';
import { AuthStore } from '../../../../core/auth/auth.store';
import { ImagesApiClient } from '../../../../core/api/images-api.client';
import { apiErrorMessage } from '../../../../core/api/api-base';
import { CartStore } from '../../data-access/cart.store';

@Component({
  selector: 'app-cart-page',
  imports: [CurrencyPipe, RouterLink],
  templateUrl: './cart-page.html',
  styleUrl: './cart-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CartPage {
  protected readonly cart = inject(CartStore);
  protected readonly auth = inject(AuthStore);
  private readonly orders = inject(OrdersApiClient);
  private readonly images = inject(ImagesApiClient);
  private readonly router = inject(Router);
  protected readonly submitting = signal(false);
  protected readonly error = signal('');
  protected imageUrl(id?: string) {
    return id ? this.images.contentUrl(id) : null;
  }
  protected checkout() {
    if (!this.auth.isAuthenticated()) {
      this.router.navigate(['/login'], { queryParams: { returnUrl: '/cart' } });
      return;
    }
    const clientId = this.auth.user()?.userId;
    if (!clientId) {
      this.error.set(
        'Your account is missing the customer profile identifier required to place an order.',
      );
      return;
    }
    this.submitting.set(true);
    this.error.set('');
    this.orders
      .create(
        clientId,
        this.cart.items().map((item) => ({ productId: item.product.id, quantity: item.quantity })),
      )
      .subscribe({
        next: () => {
          this.cart.clear();
          this.router.navigate(['/orders']);
        },
        error: (error) => {
          this.error.set(apiErrorMessage(error));
          this.submitting.set(false);
        },
      });
  }
}
