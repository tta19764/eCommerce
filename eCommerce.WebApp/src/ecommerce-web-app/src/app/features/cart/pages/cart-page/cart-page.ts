import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { apiErrorMessage } from '../../../../core/api/api-base';
import { ImagesApiClient } from '../../../../core/api/images-api.client';
import { OrdersApiClient } from '../../../../core/api/orders-api.client';
import { AuthStore } from '../../../../core/auth/auth.store';
import { OrderItemRequest } from '../../../../core/models/order.models';
import { Product } from '../../../../core/models/product.models';
import { CartStore } from '../../data-access/cart.store';

@Component({
  selector: 'app-cart-page',
  imports: [CurrencyPipe, RouterLink],
  templateUrl: './cart-page.html',
  styleUrl: './cart-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CartPage {
  private readonly orders = inject(OrdersApiClient);
  private readonly images = inject(ImagesApiClient);
  private readonly router = inject(Router);
  private readonly auth = inject(AuthStore);

  protected readonly cart = inject(CartStore);
  protected readonly submitting = signal(false);
  protected readonly error = signal('');

  protected imageUrl(product: Product): string | null {
    const imageId = product.displayImageId ?? product.imageIds[0];

    return imageId ? this.images.contentUrl(imageId) : null;
  }

  protected checkout(): void {
    if (!this.auth.isAuthenticated()) {
      this.router.navigate(['/login'], { queryParams: { returnUrl: '/cart' } });
      return;
    }

    this.submitting.set(true);
    this.error.set('');

    // The own-order endpoint derives clientId from the authenticated user's claims.
    this.orders.createOwn(this.orderItems()).subscribe({
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

  private orderItems(): OrderItemRequest[] {
    return this.cart.items().map((item) => ({
      productId: item.product.id,
      quantity: item.quantity,
    }));
  }
}
