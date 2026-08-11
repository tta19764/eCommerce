import { AppCurrencyPipe } from '../../../../shared/pipes/app-currency.pipe';
import { ChangeDetectionStrategy, Component, computed, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import { apiErrorMessage } from '../../../../core/api/api-base';
import { ImagesApiClient } from '../../../../core/api/images-api';
import { OrdersApiClient } from '../../../../core/api/orders-api';
import { AuthStore } from '../../../../core/auth/auth-store';
import { OrderItemRequest } from '../../../../core/models/order-model';
import { OrderPricingQuote, OrderPricingQuoteItem } from '../../../../core/models/order-model';
import { Product } from '../../../../core/models/product-model';
import { CartStore } from '../../data-access/cart-store';
import { catchError, combineLatest, debounceTime, map, of, startWith, Subject, Subscription, switchMap, tap, timer } from 'rxjs';

@Component({
  selector: 'app-cart-page',
  imports: [AppCurrencyPipe, RouterLink],
  templateUrl: './cart-page.html',
  styleUrl: './cart-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CartPage {
  private readonly orders = inject(OrdersApiClient);
  private readonly images = inject(ImagesApiClient);
  private readonly router = inject(Router);
  private readonly auth = inject(AuthStore);
  private readonly destroyRef = inject(DestroyRef);
  private expiryTimer?: Subscription;
  protected readonly refreshQuote = new Subject<void>();

  protected readonly cart = inject(CartStore);
  protected readonly submitting = signal(false);
  protected readonly error = signal('');
  protected readonly checkoutCurrency = signal('USD');
  protected readonly quote = signal<OrderPricingQuote | null>(null);
  protected readonly quoteLoading = signal(false);
  protected readonly quoteError = signal('');
  protected readonly quoteReady = computed(() => {
    const quote = this.quote();
    return quote !== null && !this.quoteLoading() && !this.quoteError();
  });

  constructor() {
    // switchMap unsubscribes the previous HTTP request when quantity/currency changes. This prevents a
    // slower obsolete quote from replacing newer basket state, while debounce limits provider pressure.
    combineLatest([
      toObservable(this.cart.items),
      toObservable(this.checkoutCurrency),
      this.refreshQuote.pipe(startWith(undefined)),
    ])
      .pipe(
        debounceTime(250),
        map(([items, currency]) => ({
          items: items.map((item) => ({ productId: item.product.id, quantity: item.quantity })),
          currency,
        })),
        tap(({ items }) => {
          // Clear the old amount immediately: it no longer describes the visible basket.
          this.quote.set(null);
          this.quoteError.set('');
          this.quoteLoading.set(items.length > 0);
        }),
        switchMap(({ items, currency }) =>
          items.length === 0
            ? of({ quote: null, error: '' })
            : this.orders.getPricingQuote(items, currency).pipe(
                map((quote) => ({ quote, error: '' })),
                catchError((error) => of({ quote: null, error: apiErrorMessage(error) })),
              ),
        ),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(({ quote, error }) => {
        this.expiryTimer?.unsubscribe();
        this.quote.set(quote);
        this.quoteError.set(error);
        this.quoteLoading.set(false);

        if (quote) {
          // A preview is non-binding but should not remain displayed beyond its provider freshness window.
          const delay = Math.max(0, new Date(quote.quoteExpiresOnUtc).getTime() - Date.now());
          this.expiryTimer = timer(delay)
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe(() => this.refreshQuote.next());
        }
      });
  }

  protected imageUrl(product: Product): string | null {
    const imageId = product.displayImageId ?? product.imageIds[0];

    return imageId ? this.images.contentUrl(imageId) : null;
  }

  protected checkout(): void {
    if (!this.quoteReady()) return;
    if (!this.auth.isAuthenticated()) {
      this.router.navigate(['/login'], { queryParams: { returnUrl: '/cart' } });
      return;
    }

    this.submitting.set(true);
    this.error.set('');

    // The own-order endpoint derives clientId from the authenticated user's claims.
    this.orders.createOwn(this.orderItems(), this.checkoutCurrency()).subscribe({
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

  protected selectCheckoutCurrency(event: Event): void {
    this.checkoutCurrency.set((event.target as HTMLSelectElement).value);
  }

  protected quotedLine(productId: string): OrderPricingQuoteItem | undefined {
    return this.quote()?.items.find((item) => item.productId === productId);
  }

  protected formatMinor(amountMinor: number, currency: string, minorUnitDigits: number): string {
    // Scale using server-provided currency metadata instead of assuming cents for every ISO currency.
    return new Intl.NumberFormat(undefined, {
      style: 'currency',
      currency,
      minimumFractionDigits: minorUnitDigits,
      maximumFractionDigits: minorUnitDigits,
    }).format(amountMinor / 10 ** minorUnitDigits);
  }

  private orderItems(): OrderItemRequest[] {
    return this.cart.items().map((item) => ({
      productId: item.product.id,
      quantity: item.quantity,
    }));
  }
}
