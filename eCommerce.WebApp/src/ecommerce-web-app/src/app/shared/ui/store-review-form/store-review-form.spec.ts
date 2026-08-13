import { HttpErrorResponse, provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { SellerApiClient } from '../../../core/api/seller-api';
import { StoreReviewFormComponent } from './store-review-form';

describe('StoreReviewFormComponent', () => {
  let sellerApiMock: Partial<SellerApiClient>;

  beforeEach(() => {
    sellerApiMock = {
      createStoreReview: vi.fn().mockReturnValue(of('review-555')),
    };

    TestBed.configureTestingModule({
      imports: [StoreReviewFormComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: SellerApiClient, useValue: sellerApiMock },
      ],
    });
  });

  it('submits review with context-supplied storeId and sellerOrderId', () => {
    const fixture = TestBed.createComponent(StoreReviewFormComponent);
    fixture.componentRef.setInput('storeId', 'store-abc');
    fixture.componentRef.setInput('sellerOrderId', 's-order-xyz');
    fixture.detectChanges();

    const component = fixture.componentInstance;
    (component as any).form.setValue({ rating: 4, comment: 'Solid seller service' });

    (component as any).submitReview();

    expect(sellerApiMock.createStoreReview).toHaveBeenCalledWith('store-abc', {
      sellerOrderId: 's-order-xyz',
      rating: 4,
      comment: 'Solid seller service',
    });
    expect((component as any).success()).toBe('Store review published successfully!');
  });

  it('presents domain error when createStoreReview fails', () => {
    const httpError = new HttpErrorResponse({
      status: 400,
      error: { error: { name: 'SellerOrder.NotCompleted' } },
    });
    (sellerApiMock.createStoreReview as ReturnType<typeof vi.fn>).mockReturnValue(
      throwError(() => httpError),
    );

    const fixture = TestBed.createComponent(StoreReviewFormComponent);
    fixture.componentRef.setInput('storeId', 'store-abc');
    fixture.componentRef.setInput('sellerOrderId', 's-order-xyz');
    fixture.detectChanges();

    const component = fixture.componentInstance;
    (component as any).form.setValue({ rating: 4, comment: 'Solid seller service' });

    (component as any).submitReview();

    expect((component as any).error()).toBe('SellerOrder.NotCompleted');
  });
});
