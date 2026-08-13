import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { SellerApiClient } from '../../../../core/api/seller-api';
import { PagedList } from '../../../../core/models/api-model';
import { PendingSellerApplicationResponse, SellerStatus } from '../../../../core/models/seller-model';
import { AdminSellersPage } from './admin-sellers-page';

describe('AdminSellersPage', () => {
  let sellerApiMock: Partial<SellerApiClient>;

  const mockPendingItem1: PendingSellerApplicationResponse = {
    sellerId: 'seller-1',
    status: SellerStatus.PendingReview,
    applicant: {
      userId: 'user-owner-1',
      fullName: 'Alice Smith',
      email: 'alice@example.com',
      found: true,
    },
    store: {
      storeId: 'store-1',
      slug: 'alice-crafts',
      name: 'Alice Crafts',
      description: 'Artisanal products',
      countryCode: 'US',
      defaultCurrency: 'USD',
      logoImageId: null,
      bannerImageId: null,
    },
    submittedOnUtc: '2026-08-12T10:00:00Z',
  };

  const mockPendingItemMissingUser: PendingSellerApplicationResponse = {
    sellerId: 'seller-2',
    status: SellerStatus.PendingReview,
    applicant: {
      userId: 'user-unknown-99',
      fullName: '',
      email: '',
      found: false,
    },
    store: {
      storeId: 'store-2',
      slug: 'orphan-store',
      name: 'Orphan Store',
      description: 'Unknown owner store',
      countryCode: 'CA',
      defaultCurrency: 'CAD',
      logoImageId: null,
      bannerImageId: null,
    },
    submittedOnUtc: '2026-08-13T08:00:00Z',
  };

  const mockPagedList: PagedList<PendingSellerApplicationResponse> = {
    items: [mockPendingItem1],
    page: 1,
    pageSize: 10,
    totalCount: 1,
  };

  beforeEach(() => {
    sellerApiMock = {
      getPendingSellers: vi.fn().mockReturnValue(of(mockPagedList)),
      approveSeller: vi.fn().mockReturnValue(of(undefined)),
      rejectSeller: vi.fn().mockReturnValue(of(undefined)),
    };

    TestBed.configureTestingModule({
      imports: [AdminSellersPage],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: SellerApiClient, useValue: sellerApiMock },
      ],
    });
  });

  it('loads pending applications with pagination on init', () => {
    const fixture = TestBed.createComponent(AdminSellersPage);
    fixture.detectChanges();
    const component = fixture.componentInstance;

    expect(sellerApiMock.getPendingSellers).toHaveBeenCalledWith({ page: 1, pageSize: 10 });
    expect((component as any).pendingSellers().length).toBe(1);
    expect((component as any).pendingSellers()[0].sellerId).toBe('seller-1');
  });

  it('renders enriched applicant and store details in DOM', () => {
    const fixture = TestBed.createComponent(AdminSellersPage);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;

    expect(compiled.textContent).toContain('Alice Smith');
    expect(compiled.textContent).toContain('alice@example.com');
    expect(compiled.textContent).toContain('Alice Crafts');
    expect(compiled.textContent).toContain('/store/alice-crafts');
    expect(compiled.textContent).toContain('US (USD)');
  });

  it('renders profile-consistency warning when applicant.found is false', () => {
    (sellerApiMock.getPendingSellers as any).mockReturnValue(
      of({
        items: [mockPendingItemMissingUser],
        page: 1,
        pageSize: 10,
        totalCount: 1,
      }),
    );

    const fixture = TestBed.createComponent(AdminSellersPage);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;

    expect(compiled.textContent).toContain('Profile-Consistency Warning');
    expect(compiled.textContent).toContain('user-unknown-99');
  });

  it('opens approval modal, confirms approval via sellerId, and reloads current page', () => {
    const fixture = TestBed.createComponent(AdminSellersPage);
    fixture.detectChanges();
    const component = fixture.componentInstance;

    (component as any).openApproveModal(mockPendingItem1);
    expect((component as any).approvingSeller()?.sellerId).toBe('seller-1');

    (component as any).confirmApprove();
    expect(sellerApiMock.approveSeller).toHaveBeenCalledWith('seller-1');
    expect((component as any).approvingSeller()).toBeNull();
  });

  it('rejectSeller requires non-empty reason, uses sellerId, and reloads on success', () => {
    const fixture = TestBed.createComponent(AdminSellersPage);
    fixture.detectChanges();
    const component = fixture.componentInstance;

    (component as any).openRejectModal(mockPendingItem1);
    (component as any).rejectionReason.set('   ');

    (component as any).confirmReject();
    expect(sellerApiMock.rejectSeller).not.toHaveBeenCalled();
    expect((component as any).error()).toBe('A non-empty rejection reason is required.');

    (component as any).rejectionReason.set('Incomplete store information');
    (component as any).confirmReject();

    expect(sellerApiMock.rejectSeller).toHaveBeenCalledWith('seller-1', {
      reason: 'Incomplete store information',
    });
  });

  it('setPage triggers getPendingSellers with updated page number', () => {
    (sellerApiMock.getPendingSellers as any).mockReturnValue(
      of({
        items: [mockPendingItem1],
        page: 1,
        pageSize: 10,
        totalCount: 25,
      }),
    );

    const fixture = TestBed.createComponent(AdminSellersPage);
    fixture.detectChanges();
    const component = fixture.componentInstance;

    (component as any).setPage(2);
    expect(sellerApiMock.getPendingSellers).toHaveBeenCalledWith({ page: 2, pageSize: 10 });
  });
});
