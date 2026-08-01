import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ProductsApiClient } from '../../../../core/api/products-api.client';
import { Product } from '../../../../core/models/product.models';
import { ProductCard } from '../../../../shared/ui/product-card/product-card';

@Component({
  selector: 'app-catalog-page',
  imports: [ProductCard],
  templateUrl: './catalog-page.html',
  styleUrl: './catalog-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CatalogPage {
  private readonly api = inject(ProductsApiClient);
  protected readonly products = signal<Product[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal('');
  protected readonly page = signal(1);
  protected readonly total = signal(0);
  constructor() {
    this.load();
  }
  protected load(page = 1) {
    this.loading.set(true);
    this.error.set('');
    this.api.getPage({ page, pageSize: 12 }).subscribe({
      next: (result) => {
        this.products.set(result.items);
        this.total.set(result.totalCount);
        this.page.set(result.page);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('The collection is temporarily unavailable.');
        this.loading.set(false);
      },
    });
  }
}
