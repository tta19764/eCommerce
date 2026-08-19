import { AppCurrencyPipe } from '../../../../shared/pipes/app-currency.pipe';
import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { LucideAngularModule, Store } from 'lucide-angular';
import { forkJoin } from 'rxjs';
import { ImagesApiClient } from '../../../../core/api/images-api';
import { ProductsApiClient } from '../../../../core/api/products-api';
import { SellerApiClient } from '../../../../core/api/seller-api';
import { Product, ProductReview, ProductReviewEligibility } from '../../../../core/models/product-model';
import { StoreResponse } from '../../../../core/models/seller-model';
import { AuthStore } from '../../../../core/auth/auth-store';
import { UserStore } from '../../../../core/user/user-store';
import { CartStore } from '../../../cart/data-access/cart-store';

export interface RatingDistributionRow {
  stars: number;
  count: number;
  percentage: number;
}

@Component({
  selector: 'app-product-page',
  imports: [AppCurrencyPipe, DatePipe, RouterLink, FormsModule, LucideAngularModule],
  templateUrl: './product-page.html',
  styleUrl: './product-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductPage {
  readonly StoreIcon = Store;
  private readonly api = inject(ProductsApiClient);
  private readonly sellerApi = inject(SellerApiClient);
  private readonly images = inject(ImagesApiClient);
  private readonly route = inject(ActivatedRoute);

  protected readonly auth = inject(AuthStore);
  protected readonly userStore = inject(UserStore);
  protected readonly cart = inject(CartStore);
  protected readonly Math = Math;

  protected readonly product = signal<Product | null>(null);
  protected readonly store = signal<StoreResponse | null>(null);
  protected readonly reviews = signal<ProductReview[]>([]);
  protected readonly reviewEligibility = signal<ProductReviewEligibility | null>(null);
  protected readonly loading = signal(true);
  protected readonly failed = signal(false);
  protected readonly quantity = signal(1);
  protected readonly choosingQuantity = signal(false);

  // Review Form & Rating Signals
  protected readonly newRating = signal(5);
  protected readonly hoverRating = signal(0);
  protected readonly newComment = signal('');
  protected readonly submittingReview = signal(false);
  protected readonly reviewSuccess = signal('');
  protected readonly reviewError = signal('');

  // Rating Distribution (5 to 1) computed dynamically from reviews list
  protected readonly ratingDistribution = computed<RatingDistributionRow[]>(() => {
    const list = this.reviews();
    const total = list.length;

    return [5, 4, 3, 2, 1].map((stars) => {
      const count = list.filter((r) => r.rating === stars).length;
      const percentage = total > 0 ? Math.round((count / total) * 100) : 0;
      return { stars, count, percentage };
    });
  });

  protected readonly averageRating = computed<number>(() => {
    const p = this.product();
    if (p?.rating) return p.rating;
    const list = this.reviews();
    if (!list.length) return 0;
    const sum = list.reduce((acc, r) => acc + r.rating, 0);
    return Math.round((sum / list.length) * 10) / 10;
  });

  protected readonly roundedRating = computed<number>(() => {
    return Math.round(this.averageRating());
  });

  constructor() {
    this.loadProduct();
  }

  protected imageUrl(imageId: string): string {
    return this.images.contentUrl(imageId);
  }

  protected orderedImageIds(product: Product): string[] {
    if (!product.displayImageId) {
      return product.imageIds;
    }

    return [
      product.displayImageId,
      ...product.imageIds.filter((imageId) => imageId !== product.displayImageId),
    ];
  }

  protected decreaseQuantity(): void {
    this.quantity.update((value) => Math.max(1, value - 1));
  }

  protected increaseQuantity(): void {
    const availableQuantity = this.product()?.quantity ?? 1;
    this.quantity.update((value) => Math.min(availableQuantity, value + 1));
  }

  protected addToCart(): void {
    const product = this.product();
    if (!product?.quantity) return;

    if (!this.choosingQuantity()) {
      this.choosingQuantity.set(true);
      return;
    }

    this.cart.add(product, this.quantity());
    this.quantity.set(1);
    this.choosingQuantity.set(false);
  }

  protected setStarRating(rating: number): void {
    this.newRating.set(rating);
  }

  protected setStarHover(rating: number): void {
    this.hoverRating.set(rating);
  }

  protected submitReview(): void {
    const p = this.product();
    if (!p) return;

    const rating = this.newRating();
    const comment = this.newComment().trim();

    if (rating < 1 || rating > 5) {
      this.reviewError.set('Please select a star rating from 1 to 5.');
      return;
    }

    if (comment.length < 5) {
      this.reviewError.set('Please write a review comment with at least 5 characters.');
      return;
    }

    this.submittingReview.set(true);
    this.reviewError.set('');
    this.reviewSuccess.set('');

    const userId = this.auth.user()?.id ?? '';

    this.api.createReview(p.id, { rating, comment }).subscribe({
      next: (createdId) => {
        const newReview: ProductReview = {
          id: createdId || `rev-${Date.now()}`,
          productId: p.id,
          userId: userId || 'current-user',
          reviewerName: this.userStore.getFormattedReviewerName(),
          rating,
          comment,
          createdAtUtc: new Date().toISOString(),
        };
        this.reviews.update((prev) => [newReview, ...prev]);
        this.newComment.set('');
        this.newRating.set(5);
        this.submittingReview.set(false);
        this.reviewSuccess.set('Thank you! Your review has been published successfully.');
        this.api.getById(p.id).subscribe((upd) => this.product.set(upd));
        this.api.getReviews(p.id).subscribe((revs) => this.reviews.set(revs.items || []));
        this.api.getReviewEligibility(p.id).subscribe((el) => this.reviewEligibility.set(el));
      },
      error: (err) => {
        this.submittingReview.set(false);
        const detail =
          err?.error?.error?.message ||
          err?.error?.detail ||
          err?.error?.title ||
          'Failed to publish review. Please check your eligibility and try again.';
        this.reviewError.set(detail);
      },
    });
  }

  protected deleteReview(reviewId: string): void {
    const p = this.product();
    if (!p) return;

    this.api.deleteReview(p.id, reviewId).subscribe({
      next: () => {
        this.reviews.update((prev) => prev.filter((r) => r.id !== reviewId));
      },
      error: () => {
        this.reviews.update((prev) => prev.filter((r) => r.id !== reviewId));
      },
    });
  }

  protected canDeleteReview(review: ProductReview): boolean {
    if (this.auth.isAdmin()) return true;
    if (!this.auth.isAuthenticated()) return false;
    const userApiId = this.userStore.userId();
    return !!userApiId && userApiId === review.userId;
  }

  protected reviewerName(review: ProductReview): string {
    if (review.reviewerName && review.reviewerName.trim().length > 0) {
      const name = review.reviewerName.trim();
      const parts = name.split(/\s+/).filter((p) => p.length > 0);
      if (parts.length >= 2) {
        const first = parts[0].charAt(0).toUpperCase() + parts[0].slice(1).toLowerCase();
        const lastInitial = parts[1].charAt(0).toUpperCase();
        return `${first} ${lastInitial}.`;
      }
      return parts[0].charAt(0).toUpperCase() + parts[0].slice(1).toLowerCase();
    }
    return 'Verified Customer';
  }

  protected reviewerInitials(review: ProductReview): string {
    const name = this.reviewerName(review);
    const parts = name.split(' ').filter((p) => p.length > 0);
    if (parts.length >= 2) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return name.slice(0, 2).toUpperCase();
  }

  private loadProduct(): void {
    const productId = this.route.snapshot.paramMap.get('id');

    if (!productId) {
      this.failed.set(true);
      this.loading.set(false);
      return;
    }

    const requests: Record<string, any> = {
      product: this.api.getById(productId),
      reviews: this.api.getReviews(productId),
    };

    if (this.auth.isAuthenticated()) {
      requests['eligibility'] = this.api.getReviewEligibility(productId);
    }

    forkJoin(requests).subscribe({
      next: (res: any) => {
        this.product.set(res.product);
        this.reviews.set(res.reviews?.items || []);
        if (res.eligibility) {
          this.reviewEligibility.set(res.eligibility);
        }
        this.loading.set(false);

        const storeSlugOrId = res.product?.store?.slug || res.product?.sellerId;
        if (storeSlugOrId) {
          this.sellerApi.getStoreBySlug(storeSlugOrId).subscribe({
            next: (storeData) => this.store.set(storeData),
            error: () => this.store.set(null),
          });
        }
      },
      error: () => {
        this.failed.set(true);
        this.loading.set(false);
      },
    });
  }
}
