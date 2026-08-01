import { CurrencyPipe, DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { ImagesApiClient } from '../../../../core/api/images-api.client';
import { ProductsApiClient } from '../../../../core/api/products-api.client';
import { Product, ProductReview } from '../../../../core/models/product.models';
import { CartStore } from '../../../cart/data-access/cart.store';

@Component({
  selector: 'app-product-page',
  imports: [CurrencyPipe, DatePipe, RouterLink],
  templateUrl: './product-page.html',
  styleUrl: './product-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductPage {
  private readonly api = inject(ProductsApiClient);
  private readonly images = inject(ImagesApiClient);
  protected readonly cart = inject(CartStore);
  protected readonly product = signal<Product | null>(null);
  protected readonly reviews = signal<ProductReview[]>([]);
  protected readonly loading = signal(true);
  protected readonly failed = signal(false);
  protected readonly quantity = signal(1);
  constructor() {
    const id = inject(ActivatedRoute).snapshot.paramMap.get('id')!;
    forkJoin({ product: this.api.getById(id), reviews: this.api.getReviews(id) }).subscribe({
      next: ({ product, reviews }) => {
        this.product.set(product);
        this.reviews.set(reviews.items);
        this.loading.set(false);
      },
      error: () => {
        this.failed.set(true);
        this.loading.set(false);
      },
    });
  }
  protected imageUrl(id: string) {
    return this.images.contentUrl(id);
  }
  protected decreaseQuantity() {
    this.quantity.update((value) => Math.max(1, value - 1));
  }
  protected increaseQuantity() {
    this.quantity.update((value) => Math.min(this.product()?.quantity ?? 1, value + 1));
  }
}
