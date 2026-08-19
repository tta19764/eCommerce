import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, ParamMap } from '@angular/router';
import { BehaviorSubject, of, Subject, throwError } from 'rxjs';
import { ProductsApiClient } from '../../../../core/api/products-api';
import { SellerApiClient } from '../../../../core/api/seller-api';
import { PagedList } from '../../../../core/models/api-model';
import { Product } from '../../../../core/models/product-model';
import { StoreResponse, StoreReviewResponse } from '../../../../core/models/seller-model';
import { StorePage } from './store-page';

describe('StorePage', () => {
  let fixture: ComponentFixture<StorePage>;
  let component: StorePage;
  let sellerApiMock: Partial<SellerApiClient>;
  let productsApiMock: Partial<ProductsApiClient>;
  let paramMapSubject: BehaviorSubject<ParamMap>;

  const mockStore: StoreResponse = {
    id: 'store-1',
    sellerId: 'seller-1',
    slug: 'apex-store',
    name: 'Apex Store',
    description: 'Apex store description',
    countryCode: 'US',
    defaultCurrency: 'USD',
    logoImageId: null,
    bannerImageId: null,
    averageRating: 4.8,
    reviewCount: 12,
  };

  const mockStoreBeta: StoreResponse = {
    id: 'store-2',
    sellerId: 'seller-2',
    slug: 'beta-store',
    name: 'Beta Store',
    description: 'Beta store description',
    countryCode: 'US',
    defaultCurrency: 'USD',
    logoImageId: null,
    bannerImageId: null,
    averageRating: 4.0,
    reviewCount: 3,
  };

  const mockProduct: Product = {
    id: 'prod-1',
    sellerId: 'seller-1',
    name: 'Test Product 1',
    description: 'Test description',
    price: 99.99,
    currency: 'USD',
    quantity: 10,
    productType: 'Physical',
    categoryId: 'cat-1',
    imageIds: [],
    displayImageId: null,
    rating: 4.5,
    reviewsCount: 5,
    store: null,
  };

  const mockFullReviewPage: StoreReviewResponse[] = [
    { id: 'rev-1', customerUserId: 'c-1', sellerOrderId: 'o-1', rating: 5, comment: 'Great 1', createdOnUtc: '2026-08-10T00:00:00Z' },
    { id: 'rev-2', customerUserId: 'c-2', sellerOrderId: 'o-2', rating: 5, comment: 'Great 2', createdOnUtc: '2026-08-11T00:00:00Z' },
    { id: 'rev-3', customerUserId: 'c-3', sellerOrderId: 'o-3', rating: 4, comment: 'Great 3', createdOnUtc: '2026-08-12T00:00:00Z' },
    { id: 'rev-4', customerUserId: 'c-4', sellerOrderId: 'o-4', rating: 5, comment: 'Great 4', createdOnUtc: '2026-08-13T00:00:00Z' },
    { id: 'rev-5', customerUserId: 'c-5', sellerOrderId: 'o-5', rating: 5, comment: 'Great 5', createdOnUtc: '2026-08-14T00:00:00Z' },
    { id: 'rev-6', customerUserId: 'c-6', sellerOrderId: 'o-6', rating: 5, comment: 'Great 6', createdOnUtc: '2026-08-15T00:00:00Z' },
  ];

  const mockShortReviewPage: StoreReviewResponse[] = [
    { id: 'rev-7', customerUserId: 'c-7', sellerOrderId: 'o-7', rating: 4, comment: 'Great 7', createdOnUtc: '2026-08-16T00:00:00Z' },
  ];

  beforeEach(() => {
    paramMapSubject = new BehaviorSubject<ParamMap>(convertToParamMap({ slug: 'apex-store' }));

    sellerApiMock = {
      getStoreBySlug: vi.fn().mockImplementation((slug: string) => {
        if (slug === 'beta-store') {
          return of(mockStoreBeta);
        }
        return of(mockStore);
      }),
      getStoreReviews: vi.fn().mockReturnValue(of(mockFullReviewPage)),
    };

    productsApiMock = {
      getPage: vi.fn().mockImplementation((query: any) =>
        of({
          items: [mockProduct],
          page: query?.page ?? 1,
          pageSize: 12,
          totalCount: 25,
        }),
      ),
    };

    TestBed.configureTestingModule({
      imports: [StorePage],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: SellerApiClient, useValue: sellerApiMock },
        { provide: ProductsApiClient, useValue: productsApiMock },
        {
          provide: ActivatedRoute,
          useValue: {
            paramMap: paramMapSubject.asObservable(),
            queryParamMap: of(convertToParamMap({})),
          },
        },
      ],
    });
  });

  function createComponent(): void {
    fixture = TestBed.createComponent(StorePage);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }

  it('1. Store loading requests products with store.sellerId', () => {
    createComponent();

    expect(sellerApiMock.getStoreBySlug).toHaveBeenCalledWith('apex-store');
    expect(productsApiMock.getPage).toHaveBeenCalledWith({
      sellerId: 'seller-1',
      page: 1,
      pageSize: 12,
    });
  });

  it('2. Products render correctly', () => {
    createComponent();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('app-product-card')).not.toBeNull();
    expect(compiled.textContent).toContain('Store Products');
    expect(compiled.textContent).toContain('25 products');
  });

  it('3. Empty-product state renders', () => {
    (productsApiMock.getPage as ReturnType<typeof vi.fn>).mockReturnValue(
      of({ items: [], page: 1, pageSize: 12, totalCount: 0 }),
    );

    createComponent();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('No products available from this seller yet.');
  });

  it('4. Product pagination retains the correct sellerId', () => {
    createComponent();

    (component as any).onProductPageChange(2);
    fixture.detectChanges();

    expect(productsApiMock.getPage).toHaveBeenLastCalledWith({
      sellerId: 'seller-1',
      page: 2,
      pageSize: 12,
    });
  });

  it('5. The first review page loads', () => {
    createComponent();

    expect(sellerApiMock.getStoreReviews).toHaveBeenCalledWith('store-1', { page: 1, pageSize: 6 });
    expect((component as any).reviews().length).toBe(6);
    expect((component as any).hasMoreReviews()).toBe(true);
  });

  it('6. "Load more" appends reviews', () => {
    createComponent();

    (sellerApiMock.getStoreReviews as ReturnType<typeof vi.fn>).mockReturnValue(
      of(mockShortReviewPage),
    );

    (component as any).loadMoreReviews();
    fixture.detectChanges();

    expect(sellerApiMock.getStoreReviews).toHaveBeenCalledWith('store-1', { page: 2, pageSize: 6 });
    expect((component as any).reviews().length).toBe(7);
  });

  it('7. "Load more" disappears after a short page', () => {
    createComponent();

    (sellerApiMock.getStoreReviews as ReturnType<typeof vi.fn>).mockReturnValue(
      of(mockShortReviewPage),
    );

    (component as any).loadMoreReviews();
    fixture.detectChanges();

    expect((component as any).hasMoreReviews()).toBe(false);
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('button[aria-label="Load more reviews"]')).toBeNull();
  });

  it('8. Route slug changes reset product and review pagination', () => {
    createComponent();

    // Advance product page
    (component as any).onProductPageChange(2);
    expect((component as any).productsPage()).toBe(2);

    // Switch route slug
    paramMapSubject.next(convertToParamMap({ slug: 'beta-store' }));
    fixture.detectChanges();

    expect((component as any).currentSlug()).toBe('beta-store');
    expect((component as any).productsPage()).toBe(1);
    expect((component as any).reviewsPage()).toBe(1);
    expect(sellerApiMock.getStoreBySlug).toHaveBeenCalledWith('beta-store');
    expect(productsApiMock.getPage).toHaveBeenLastCalledWith({
      sellerId: 'seller-2',
      page: 1,
      pageSize: 12,
    });
  });

  it('9. Stale responses cannot replace data for a newer slug', () => {
    const apexStoreSubject = new Subject<StoreResponse>();
    (sellerApiMock.getStoreBySlug as ReturnType<typeof vi.fn>).mockImplementation((slug: string) => {
      if (slug === 'apex-store') {
        return apexStoreSubject.asObservable();
      }
      return of(mockStoreBeta);
    });

    createComponent();

    // Trigger beta-store route before apex-store resolves
    paramMapSubject.next(convertToParamMap({ slug: 'beta-store' }));
    fixture.detectChanges();

    expect((component as any).store()?.name).toBe('Beta Store');

    // Late emission from old apex-store request
    apexStoreSubject.next(mockStore);
    fixture.detectChanges();

    // Should still be Beta Store
    expect((component as any).store()?.name).toBe('Beta Store');
  });

  it('10. Review creation refreshes store and reviews while preserving products', () => {
    createComponent();

    (productsApiMock.getPage as ReturnType<typeof vi.fn>).mockClear();
    (sellerApiMock.getStoreBySlug as ReturnType<typeof vi.fn>).mockClear();
    (sellerApiMock.getStoreReviews as ReturnType<typeof vi.fn>).mockClear();

    (component as any).onReviewCreated();
    fixture.detectChanges();

    expect(sellerApiMock.getStoreBySlug).toHaveBeenCalledWith('apex-store');
    expect(sellerApiMock.getStoreReviews).toHaveBeenCalledWith('store-1', { page: 1, pageSize: 6 });
    expect(productsApiMock.getPage).not.toHaveBeenCalled();
  });

  it('11. Product or review failures do not incorrectly produce the store-not-found state', () => {
    (productsApiMock.getPage as ReturnType<typeof vi.fn>).mockReturnValue(
      throwError(() => new Error('Products service error')),
    );
    (sellerApiMock.getStoreReviews as ReturnType<typeof vi.fn>).mockReturnValue(
      throwError(() => new Error('Reviews service error')),
    );

    createComponent();

    expect((component as any).notFound()).toBe(false);
    expect((component as any).store()?.name).toBe('Apex Store');
    expect((component as any).productsError()).toBe('Failed to load store products.');
    expect((component as any).reviewsError()).toBe('Failed to load store reviews.');

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Failed to load store products.');
    expect(compiled.textContent).toContain('Failed to load store reviews.');
  });
});
