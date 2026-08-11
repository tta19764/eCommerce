import { AppCurrencyPipe } from '../../pipes/app-currency.pipe';
import { ChangeDetectionStrategy, Component, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ImagesApiClient } from '../../../core/api/images-api';
import { Product } from '../../../core/models/product-model';
import { CartStore } from '../../../features/cart/data-access/cart-store';

@Component({
  selector: 'app-product-card',
  imports: [AppCurrencyPipe, RouterLink],
  templateUrl: './product-card.html',
  styleUrl: './product-card.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
/** Reusable catalog summary that emits cart intent without treating its product snapshot as checkout authority. */
export class ProductCard {
  readonly product = input.required<Product>();
  private readonly images = inject(ImagesApiClient);
  protected readonly cart = inject(CartStore);
  protected readonly choosingQuantity = signal(false);
  protected readonly quantity = signal(1);

  protected imageUrl() {
    const product = this.product();
    const id = product.displayImageId ?? product.imageIds[0];
    return id ? this.images.contentUrl(id) : null;
  }

  protected decreaseQuantity(): void {
    this.quantity.update((value) => Math.max(1, value - 1));
  }

  protected increaseQuantity(): void {
    this.quantity.update((value) => Math.min(this.product().quantity, value + 1));
  }

  protected addToCart(): void {
    if (!this.product().quantity) {
      return;
    }

    if (!this.choosingQuantity()) {
      this.choosingQuantity.set(true);
      return;
    }

    this.cart.add(this.product(), this.quantity());
    this.quantity.set(1);
    this.choosingQuantity.set(false);
  }
}
