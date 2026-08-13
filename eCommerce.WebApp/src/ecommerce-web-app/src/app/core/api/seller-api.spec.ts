import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { SellerApiClient } from './seller-api';
import { PagedList } from '../models/api-model';
import {
  PendingSellerApplicationResponse,
  SellerResponse,
  SellerStatus,
  StoreResponse,
} from '../models/seller-model';

describe('SellerApiClient', () => {
  let client: SellerApiClient;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [SellerApiClient, provideHttpClient(), provideHttpClientTesting()],
    });
    client = TestBed.inject(SellerApiClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('createApplication sends POST request and unwraps ApiResponse sellerId', () => {
    const reqData = {
      slug: 'test-store',
      name: 'Test Store',
      description: 'A test store',
      countryCode: 'US',
      defaultCurrency: 'USD',
    };

    client.createApplication(reqData).subscribe((sellerId) => {
      expect(sellerId).toBe('seller-123');
    });

    const req = httpMock.expectOne((r) => r.url.endsWith('/seller-api/v1/sellers/own/application'));
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(reqData);

    req.flush({ data: 'seller-123', error: null });
  });

  it('getOwnSeller sends GET request and returns SellerResponse', () => {
    const mockSeller: SellerResponse = {
      id: 'seller-123',
      ownerUserId: 'user-456',
      status: SellerStatus.Active,
      rejectionReason: null,
      createdOnUtc: '2026-08-12T00:00:00Z',
      reviewedOnUtc: '2026-08-12T01:00:00Z',
    };

    client.getOwnSeller().subscribe((seller) => {
      expect(seller.id).toBe('seller-123');
      expect(seller.status).toBe(SellerStatus.Active);
    });

    const req = httpMock.expectOne((r) => r.url.endsWith('/seller-api/v1/sellers/own'));
    expect(req.request.method).toBe('GET');

    req.flush({ data: mockSeller, error: null });
  });

  it('getPendingSellers sends GET request with page query params', () => {
    const mockPendingList: PagedList<PendingSellerApplicationResponse> = {
      items: [
        {
          sellerId: 'seller-1',
          status: SellerStatus.PendingReview,
          applicant: {
            userId: 'user-1',
            fullName: 'Jane Doe',
            email: 'jane@example.com',
            found: true,
          },
          store: {
            storeId: 'store-1',
            slug: 'jane-store',
            name: 'Jane Store',
            description: 'Handmade crafts',
            countryCode: 'US',
            defaultCurrency: 'USD',
            logoImageId: null,
            bannerImageId: null,
          },
          submittedOnUtc: '2026-08-12T00:00:00Z',
        },
      ],
      page: 2,
      pageSize: 5,
      totalCount: 1,
    };

    client.getPendingSellers({ page: 2, pageSize: 5 }).subscribe((res) => {
      expect(res.page).toBe(2);
      expect(res.pageSize).toBe(5);
      expect(res.totalCount).toBe(1);
      expect(res.items.length).toBe(1);
      expect(res.items[0].sellerId).toBe('seller-1');
      expect(res.items[0].applicant.fullName).toBe('Jane Doe');
      expect(res.items[0].store.name).toBe('Jane Store');
    });

    const req = httpMock.expectOne((r) =>
      r.urlWithParams.includes('/seller-api/v1/sellers/pending?page=2&pageSize=5'),
    );
    expect(req.request.method).toBe('GET');

    req.flush({ data: mockPendingList, error: null });
  });

  it('approveSeller sends POST request returning 204 No Content', () => {
    client.approveSeller('seller-123').subscribe(() => {
      expect(true).toBe(true);
    });

    const req = httpMock.expectOne((r) => r.url.endsWith('/seller-api/v1/sellers/seller-123/approve'));
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({});

    req.flush(null, { status: 204, statusText: 'No Content' });
  });

  it('rejectSeller sends POST request with rejection reason body', () => {
    client.rejectSeller('seller-123', { reason: 'Incomplete info' }).subscribe(() => {
      expect(true).toBe(true);
    });

    const req = httpMock.expectOne((r) => r.url.endsWith('/seller-api/v1/sellers/seller-123/reject'));
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ reason: 'Incomplete info' });

    req.flush(null, { status: 204, statusText: 'No Content' });
  });

  it('getStoreBySlug fetches store by slug', () => {
    const mockStore: StoreResponse = {
      id: 'store-1',
      sellerId: 'seller-1',
      slug: 'my-store',
      name: 'My Store',
      description: 'Desc',
      countryCode: 'US',
      defaultCurrency: 'USD',
      logoImageId: null,
      bannerImageId: null,
      averageRating: 4.5,
      reviewCount: 2,
    };

    client.getStoreBySlug('my-store').subscribe((store) => {
      expect(store.slug).toBe('my-store');
    });

    const req = httpMock.expectOne((r) => r.url.endsWith('/seller-api/v1/stores/my-store'));
    expect(req.request.method).toBe('GET');

    req.flush({ data: mockStore, error: null });
  });

  it('createStoreReview sends POST request to store review endpoint', () => {
    const reviewReq = {
      sellerOrderId: 'order-1',
      rating: 5,
      comment: 'Great store!',
    };

    client.createStoreReview('store-1', reviewReq).subscribe((reviewId) => {
      expect(reviewId).toBe('rev-100');
    });

    const req = httpMock.expectOne((r) => r.url.endsWith('/seller-api/v1/stores/store-1/reviews'));
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(reviewReq);

    req.flush({ data: 'rev-100', error: null });
  });
});
