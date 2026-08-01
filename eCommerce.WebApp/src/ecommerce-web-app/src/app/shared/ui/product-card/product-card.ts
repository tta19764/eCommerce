import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ImagesApiClient } from '../../../core/api/images-api.client';
import { Product } from '../../../core/models/product.models';
import { CartStore } from '../../../features/cart/data-access/cart.store';

@Component({
  selector: 'app-product-card',
  imports: [CurrencyPipe, RouterLink],
  templateUrl: './product-card.html',
  styleUrl: './product-card.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductCard {
  readonly product = input.required<Product>();
  private readonly images = inject(ImagesApiClient);
  protected readonly cart = inject(CartStore);
  protected imageUrl() {
    const id = this.product().imageIds[0];
    return id ? this.images.contentUrl(id) : null;
  }
}
