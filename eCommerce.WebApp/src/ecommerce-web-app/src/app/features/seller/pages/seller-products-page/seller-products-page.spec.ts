import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { ProductsApiClient } from '../../../../core/api/products-api';
import { SellerApiClient } from '../../../../core/api/seller-api';
import { UsersApiClient } from '../../../../core/api/users-api';
import { SellerResponse, SellerStatus } from '../../../../core/models/seller-model';
import { UserProfile } from '../../../../core/models/user-model';
import { SellerProductsPage } from './seller-products-page';

describe('SellerProductsPage', () => {
  let sellerApiMock: Partial<SellerApiClient>;
  let usersApiMock: Partial<UsersApiClient>;
  let productsApiMock: Partial<ProductsApiClient>;

  const mockProfile: UserProfile = {
    id: 'user-prof-1',
    firstName: 'Jane',
    lastName: 'Doe',
    fullName: 'Jane Doe',
    email: 'jane@example.com',
    imageId: null,
  };

  beforeEach(() => {
    sellerApiMock = {
      getOwnSeller: vi.fn(),
      createApplication: vi.fn(),
    };

    usersApiMock = {
      getOwn: vi.fn().mockReturnValue(of(mockProfile)),
    };

    productsApiMock = {
      getCategories: vi.fn().mockReturnValue(of([])),
      getTypes: vi.fn().mockReturnValue(of([])),
      getPage: vi.fn().mockReturnValue(of({ items: [], page: 1, pageSize: 50, totalCount: 0 })),
    };

    TestBed.configureTestingModule({
      imports: [SellerProductsPage],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: SellerApiClient, useValue: sellerApiMock },
        { provide: UsersApiClient, useValue: usersApiMock },
        { provide: ProductsApiClient, useValue: productsApiMock },
      ],
    });
  });

  it('maps 404 error from getOwnSeller to no_application portal state', () => {
    (sellerApiMock.getOwnSeller as ReturnType<typeof vi.fn>).mockReturnValue(
      throwError(() => ({ status: 404 })),
    );

    const fixture = TestBed.createComponent(SellerProductsPage);
    fixture.detectChanges();
    const component = fixture.componentInstance;

    expect((component as any).portalState()).toBe('no_application');
  });

  it('maps SellerStatus.PendingReview (0) to pending portal state', () => {
    const mockSeller: SellerResponse = {
      id: 'seller-99',
      ownerUserId: 'user-prof-1',
      status: SellerStatus.PendingReview,
      rejectionReason: null,
      createdOnUtc: '2026-08-12T00:00:00Z',
      reviewedOnUtc: null,
    };
    (sellerApiMock.getOwnSeller as ReturnType<typeof vi.fn>).mockReturnValue(of(mockSeller));

    const fixture = TestBed.createComponent(SellerProductsPage);
    fixture.detectChanges();
    const component = fixture.componentInstance;

    expect((component as any).portalState()).toBe('pending');
    expect((component as any).sellerId()).toBe('seller-99');
  });

  it('maps SellerStatus.Rejected (2) to rejected portal state with reason', () => {
    const mockSeller: SellerResponse = {
      id: 'seller-99',
      ownerUserId: 'user-prof-1',
      status: SellerStatus.Rejected,
      rejectionReason: 'Invalid business documentation',
      createdOnUtc: '2026-08-12T00:00:00Z',
      reviewedOnUtc: '2026-08-12T01:00:00Z',
    };
    (sellerApiMock.getOwnSeller as ReturnType<typeof vi.fn>).mockReturnValue(of(mockSeller));

    const fixture = TestBed.createComponent(SellerProductsPage);
    fixture.detectChanges();
    const component = fixture.componentInstance;

    expect((component as any).portalState()).toBe('rejected');
    expect((component as any).seller()?.rejectionReason).toBe('Invalid business documentation');
  });
});
