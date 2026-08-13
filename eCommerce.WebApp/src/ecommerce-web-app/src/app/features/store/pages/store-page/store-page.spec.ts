import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap } from '@angular/router';
import { of, throwError } from 'rxjs';
import { SellerApiClient } from '../../../../core/api/seller-api';
import { StoreResponse, StoreReviewResponse } from '../../../../core/models/seller-model';
import { StorePage } from './store-page';

describe('StorePage', () => {
  let sellerApiMock: Partial<SellerApiClient>;

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
    reviewCount: 1,
  };

  const mockReviews: StoreReviewResponse[] = [
    {
      id: 'rev-1',
      customerUserId: 'cust-1',
      sellerOrderId: 'order-1',
      rating: 5,
      comment: 'Top quality products',
      createdOnUtc: '2026-08-12T00:00:00Z',
    },
  ];

  beforeEach(() => {
    sellerApiMock = {
      getStoreBySlug: vi.fn().mockReturnValue(of(mockStore)),
      getStoreReviews: vi.fn().mockReturnValue(of(mockReviews)),
    };

    TestBed.configureTestingModule({
      imports: [StorePage],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: SellerApiClient, useValue: sellerApiMock },
        {
          provide: ActivatedRoute,
          useValue: {
            paramMap: of(convertToParamMap({ slug: 'apex-store' })),
            queryParamMap: of(convertToParamMap({})),
          },
        },
      ],
    });
  });

  it('loads store by slug on route init and fetches store reviews', () => {
    const fixture = TestBed.createComponent(StorePage);
    fixture.detectChanges();
    const component = fixture.componentInstance;

    expect(sellerApiMock.getStoreBySlug).toHaveBeenCalledWith('apex-store');
    expect((component as any).store()?.name).toBe('Apex Store');
    expect(sellerApiMock.getStoreReviews).toHaveBeenCalledWith('store-1', { page: 1, pageSize: 50 });
    expect((component as any).reviews().length).toBe(1);
  });

  it('handles 404 error when store is missing or inactive', () => {
    (sellerApiMock.getStoreBySlug as ReturnType<typeof vi.fn>).mockReturnValue(
      throwError(() => ({ status: 404 })),
    );

    const fixture = TestBed.createComponent(StorePage);
    fixture.detectChanges();
    const component = fixture.componentInstance;

    expect((component as any).notFound()).toBe(true);
  });
});
