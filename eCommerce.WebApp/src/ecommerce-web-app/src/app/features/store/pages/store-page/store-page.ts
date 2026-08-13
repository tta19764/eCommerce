import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { apiErrorMessage } from '../../../../core/api/api-base';
import { ImagesApiClient } from '../../../../core/api/images-api';
import { SellerApiClient } from '../../../../core/api/seller-api';
import { StoreResponse, StoreReviewResponse } from '../../../../core/models/seller-model';
import { StoreReviewFormComponent } from '../../../../shared/ui/store-review-form/store-review-form';

@Component({
  selector: 'app-store-page',
  standalone: true,
  imports: [StoreReviewFormComponent],
  templateUrl: './store-page.html',
  styleUrl: './store-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StorePage implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly sellerApi = inject(SellerApiClient);
  private readonly imagesApi = inject(ImagesApiClient);

  protected readonly store = signal<StoreResponse | null>(null);
  protected readonly reviews = signal<StoreReviewResponse[]>([]);
  protected readonly loading = signal(true);
  protected readonly notFound = signal(false);
  protected readonly error = signal('');

  // Context input from route query params
  protected readonly sellerOrderId = signal<string | null>(null);

  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      const slug = params.get('slug');
      if (slug) {
        this.loadStore(slug);
      }
    });

    this.route.queryParamMap.subscribe((queryParams) => {
      this.sellerOrderId.set(queryParams.get('sellerOrderId'));
    });
  }

  protected loadStore(slug: string): void {
    this.loading.set(true);
    this.notFound.set(false);
    this.error.set('');

    this.sellerApi.getStoreBySlug(slug).subscribe({
      next: (storeData) => {
        this.store.set(storeData);
        this.loading.set(false);
        this.loadReviews(storeData.id);
      },
      error: (err) => {
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

  protected loadReviews(storeId: string): void {
    this.sellerApi.getStoreReviews(storeId, { page: 1, pageSize: 50 }).subscribe({
      next: (reviewList) => {
        this.reviews.set(reviewList);
      },
      error: (err) => {
        console.error('[StorePage loadReviews error]:', err);
      },
    });
  }

  protected onReviewCreated(): void {
    const currentStore = this.store();
    if (currentStore) {
      this.loadStore(currentStore.slug);
    }
  }

  protected imageUrl(imageId: string | null): string | null {
    return imageId ? this.imagesApi.contentUrl(imageId) : null;
  }
}
