import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { ProductsApiClient } from '../../../../core/api/products-api.client';
import {
  Product,
  ProductCategory,
  ProductSortBy,
  ProductType,
  ProductTypeOption,
} from '../../../../core/models/product.models';
import { ProductCard } from '../../../../shared/ui/product-card/product-card';

@Component({
  selector: 'app-catalog-page',
  imports: [ProductCard, ReactiveFormsModule],
  templateUrl: './catalog-page.html',
  styleUrl: './catalog-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CatalogPage {
  private readonly api = inject(ProductsApiClient);

  protected readonly products = signal<Product[]>([]);
  protected readonly categories = signal<ProductCategory[]>([]);
  protected readonly productTypes = signal<ProductTypeOption[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal('');
  protected readonly page = signal(1);
  protected readonly total = signal(0);
  protected readonly sortOptions: ProductSortBy[] = ['Default', 'Name', 'Price', 'Rating'];

  protected readonly filterForm = new FormGroup({
    query: new FormControl('', { nonNullable: true }),
    categoryId: new FormControl('', { nonNullable: true }),
    includeSubcategories: new FormControl(true, { nonNullable: true }),
    productType: new FormControl('', { nonNullable: true }),
    minPrice: new FormControl<number | null>(null),
    maxPrice: new FormControl<number | null>(null),
    minRating: new FormControl<number | null>(null),
    inStock: new FormControl(false, { nonNullable: true }),
    sortBy: new FormControl<ProductSortBy>('Default', { nonNullable: true }),
    sortDescending: new FormControl(false, { nonNullable: true }),
  });

  constructor() {
    this.loadCategories();
    this.loadProductTypes();
    this.load();
  }

  protected applyFilters(): void {
    this.load(1);
  }

  protected clearFilters(): void {
    this.filterForm.reset({
      query: '',
      categoryId: '',
      includeSubcategories: true,
      productType: '',
      minPrice: null,
      maxPrice: null,
      minRating: null,
      inStock: false,
      sortBy: 'Default',
      sortDescending: false,
    });
    this.load(1);
  }

  protected load(page = 1): void {
    this.loading.set(true);
    this.error.set('');
    const filters = this.filterForm.getRawValue();

    this.api
      .getPage({
        page,
        pageSize: 12,
        query: filters.query.trim() || null,
        categoryId: filters.categoryId || null,
        includeSubcategories: filters.includeSubcategories,
        productType: (filters.productType as ProductType | '') || null,
        minPrice: filters.minPrice,
        maxPrice: filters.maxPrice,
        minRating: filters.minRating,
        inStock: filters.inStock || null,
        sortBy: filters.sortBy,
        sortDescending: filters.sortDescending,
      })
      .subscribe({
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

  private loadCategories(): void {
    this.api.getCategories().subscribe({
      next: (categories) => this.categories.set(categories),
      error: () => this.categories.set([]),
    });
  }

  private loadProductTypes(): void {
    this.api.getTypes().subscribe({
      next: (types) => this.productTypes.set(types),
      error: () => this.productTypes.set([]),
    });
  }
}
