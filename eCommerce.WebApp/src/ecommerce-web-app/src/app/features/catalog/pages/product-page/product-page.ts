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
  private readonly route = inject(ActivatedRoute);

  protected readonly cart = inject(CartStore);
  protected readonly product = signal<Product | null>(null);
  protected readonly reviews = signal<ProductReview[]>([]);
  protected readonly loading = signal(true);
  protected readonly failed = signal(false);
  protected readonly quantity = signal(1);

  constructor() {
    this.loadProduct();
  }

  protected imageUrl(imageId: string): string {
    return this.images.contentUrl(imageId);
  }

  protected decreaseQuantity(): void {
    this.quantity.update((value) => Math.max(1, value - 1));
  }

  protected increaseQuantity(): void {
    const availableQuantity = this.product()?.quantity ?? 1;

    this.quantity.update((value) => Math.min(availableQuantity, value + 1));
  }

  private loadProduct(): void {
    const productId = this.route.snapshot.paramMap.get('id');

    if (!productId) {
      this.failed.set(true);
      this.loading.set(false);
      return;
    }

    forkJoin({
      product: this.api.getById(productId),
      reviews: this.api.getReviews(productId),
    }).subscribe({
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
}
