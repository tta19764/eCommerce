import { Component, DestroyRef, inject, OnInit, signal, ChangeDetectionStrategy } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import { LucideAngularModule, Store, Package, MessageSquare } from 'lucide-angular';
import { apiErrorMessage } from '../../../../core/api/api-base';
import { ImagesApiClient } from '../../../../core/api/images-api';
import { ProductsApiClient } from '../../../../core/api/products-api';
import { SellerApiClient } from '../../../../core/api/seller-api';
import { Product } from '../../../../core/models/product-model';
import { StoreResponse, StoreReviewResponse } from '../../../../core/models/seller-model';
import { ProductCard } from '../../../../shared/ui/product-card/product-card';
import { StoreReviewFormComponent } from '../../../../shared/ui/store-review-form/store-review-form';

@Component({
  selector: 'app-store-page',
  standalone: true,
  imports: [ProductCard, StoreReviewFormComponent, LucideAngularModule],
  templateUrl: './store-page.html',
  styleUrl: './store-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StorePage implements OnInit {
  readonly StoreIcon = Store;
  readonly PackageIcon = Package;
  readonly MessageSquareIcon = MessageSquare;
  private readonly route = inject(ActivatedRoute);
  private readonly sellerApi = inject(SellerApiClient);
  private readonly productsApi = inject(ProductsApiClient);
  private readonly imagesApi = inject(ImagesApiClient);
  private readonly destroyRef = inject(DestroyRef);

  // Store state
  protected readonly store = signal<StoreResponse | null>(null);
  protected readonly loading = signal(true);
  protected readonly notFound = signal(false);
  protected readonly error = signal('');

  // Context input from route query params
  protected readonly sellerOrderId = signal<string | null>(null);
  protected readonly currentSlug = signal<string | null>(null);

  // Products state
  protected readonly products = signal<Product[]>([]);
  protected readonly productsLoading = signal(false);
  protected readonly productsError = signal('');
  protected readonly productsPage = signal(1);
  protected readonly productsPageSize = signal(12);
  protected readonly productsTotalCount = signal(0);

  // Reviews state
  protected readonly reviews = signal<StoreReviewResponse[]>([]);
  protected readonly reviewsLoading = signal(false);
  protected readonly reviewsLoadingMore = signal(false);
  protected readonly reviewsError = signal('');
  protected readonly reviewsPage = signal(1);
  protected readonly reviewsPageSize = signal(6);
  protected readonly hasMoreReviews = signal(false);

  ngOnInit(): void {
    this.route.paramMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((params) => {
      const slug = params.get('slug');
      if (slug) {
        this.loadStore(slug);
      }
    });

    this.route.queryParamMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((queryParams) => {
      this.sellerOrderId.set(queryParams.get('sellerOrderId'));
    });
  }

  protected loadStore(slug: string): void {
    this.currentSlug.set(slug);
    this.loading.set(true);
    this.notFound.set(false);
    this.error.set('');

    // Reset product and review state when route slug changes
    this.resetProductState();
    this.resetReviewState();

    this.sellerApi.getStoreBySlug(slug).subscribe({
      next: (storeData) => {
        if (this.currentSlug() !== slug) {
          return;
        }
        this.store.set(storeData);
        this.loading.set(false);

        // Load store products and reviews after store resolves
        this.loadProducts(storeData.sellerId, 1);
        this.loadReviews(storeData.id, 1, false);
      },
      error: (err) => {
        if (this.currentSlug() !== slug) {
          return;
        }
        console.error('[StorePage loadStore error]:', err);
        this.loading.set(false);
        if (err?.status === 404) {
          this.notFound.set(true);
        } else {
          this.error.set(apiErrorMessage(err));
        }
      },
    });
  }

  protected loadProducts(sellerId: string, page = 1): void {
    const slug = this.currentSlug();
    this.productsLoading.set(true);
    this.productsError.set('');

    this.productsApi
      .getPage({
        sellerId,
        page,
        pageSize: this.productsPageSize(),
      })
      .subscribe({
        next: (result) => {
          if (this.currentSlug() !== slug) {
            return;
          }
          this.products.set(result.items);
          this.productsPage.set(result.page);
          this.productsTotalCount.set(result.totalCount);
          this.productsLoading.set(false);
        },
        error: (err) => {
          if (this.currentSlug() !== slug) {
            return;
          }
          console.error('[StorePage loadProducts error]:', err);
          this.productsLoading.set(false);
          this.productsError.set('Failed to load store products.');
        },
      });
  }

  protected onProductPageChange(newPage: number): void {
    const currentStore = this.store();
    if (
      currentStore &&
      newPage >= 1 &&
      newPage !== this.productsPage() &&
      newPage <= this.totalProductPages()
    ) {
      this.loadProducts(currentStore.sellerId, newPage);
    }
  }

  protected loadReviews(storeId: string, page = 1, append = false): void {
    const slug = this.currentSlug();

    if (append) {
      if (this.reviewsLoadingMore()) {
        return;
      }
      this.reviewsLoadingMore.set(true);
    } else {
      this.reviewsLoading.set(true);
      this.reviewsError.set('');
    }

    this.sellerApi
      .getStoreReviews(storeId, { page, pageSize: this.reviewsPageSize() })
      .subscribe({
        next: (reviewList) => {
          if (this.currentSlug() !== slug) {
            return;
          }
          if (append) {
            this.reviews.update((prev) => [
              ...prev,
              ...reviewList.filter((r) => !prev.some((p) => p.id === r.id)),
            ]);
            this.reviewsPage.set(page);
            this.reviewsLoadingMore.set(false);
          } else {
            this.reviews.set(reviewList);
            this.reviewsPage.set(1);
            this.reviewsLoading.set(false);
          }
          this.hasMoreReviews.set(reviewList.length === this.reviewsPageSize());
        },
        error: (err) => {
          if (this.currentSlug() !== slug) {
            return;
          }
          console.error('[StorePage loadReviews error]:', err);
          if (append) {
            this.reviewsLoadingMore.set(false);
          } else {
            this.reviewsLoading.set(false);
            this.reviewsError.set('Failed to load store reviews.');
          }
        },
      });
  }

  protected loadMoreReviews(): void {
    const currentStore = this.store();
    if (currentStore && !this.reviewsLoadingMore() && this.hasMoreReviews()) {
      this.loadReviews(currentStore.id, this.reviewsPage() + 1, true);
    }
  }

  protected onReviewCreated(): void {
    const currentStore = this.store();
    if (currentStore) {
      this.sellerApi.getStoreBySlug(currentStore.slug).subscribe({
        next: (storeData) => {
          if (this.currentSlug() === currentStore.slug) {
            this.store.set(storeData);
          }
        },
      });
      this.loadReviews(currentStore.id, 1, false);
    }
  }

  protected totalProductPages(): number {
    return Math.ceil(this.productsTotalCount() / this.productsPageSize()) || 1;
  }

  protected imageUrl(imageId: string | null): string | null {
    return imageId ? this.imagesApi.contentUrl(imageId) : null;
  }

  private resetProductState(): void {
    this.products.set([]);
    this.productsPage.set(1);
    this.productsTotalCount.set(0);
    this.productsLoading.set(false);
    this.productsError.set('');
  }

  private resetReviewState(): void {
    this.reviews.set([]);
    this.reviewsPage.set(1);
    this.reviewsLoading.set(false);
    this.reviewsLoadingMore.set(false);
    this.reviewsError.set('');
    this.hasMoreReviews.set(false);
  }
}
