import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ComponentRef } from '@angular/core';
import { provideRouter } from '@angular/router';
import { vi } from 'vitest';
import { ProductCard } from './product-card';
import { ImagesApiClient } from '../../../core/api/images-api';
import { SellerApiClient } from '../../../core/api/seller-api';
import { CartStore } from '../../../features/cart/data-access/cart-store';
import { Product } from '../../../core/models/product-model';
import { of } from 'rxjs';

describe('ProductCard', () => {
  let fixture: ComponentFixture<ProductCard>;
  let component: ProductCard;
  let componentRef: ComponentRef<ProductCard>;

  const mockProductWithStore: Product = {
    id: 'prod-1',
    name: 'Keyboard',
    description: 'Mechanical keyboard',
    price: 99.99,
    currency: 'USD',
    quantity: 10,
    sellerId: 'seller-1',
    categoryId: 'cat-1',
    productType: 'Physical',
    imageIds: [],
    displayImageId: null,
    rating: 4.5,
    reviewsCount: 12,
    store: {
      id: 'store-1',
      name: 'Apex Keyboards',
      slug: 'apex-keyboards',
    },
  };

  const mockProductWithoutStore: Product = {
    ...mockProductWithStore,
    store: null,
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProductCard],
      providers: [
        provideRouter([]),
        {
          provide: ImagesApiClient,
          useValue: { contentUrl: (id: string) => `http://localhost/images/${id}` },
        },
        {
          provide: SellerApiClient,
          useValue: { getStoreBySlug: () => of(null) },
        },
        {
          provide: CartStore,
          useValue: { add: vi.fn() },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ProductCard);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;
  });

  it('renders store name linking to store slug when store object is present on product', () => {
    componentRef.setInput('product', mockProductWithStore);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const storeLink = compiled.querySelector('a[href*="/store/apex-keyboards"]');
    expect(storeLink).toBeTruthy();
    expect(storeLink?.textContent?.trim()).toBe('Apex Keyboards');
  });

  it('falls back gracefully when store property is null', () => {
    componentRef.setInput('product', mockProductWithoutStore);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const storeLink = compiled.querySelector('a[href*="/store"]');
    expect(storeLink).toBeFalsy();
  });

  it('overrides store name with explicit input when provided', () => {
    componentRef.setInput('product', mockProductWithStore);
    componentRef.setInput('storeName', 'Custom Store Name');
    componentRef.setInput('storeSlug', 'custom-slug');
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const storeLink = compiled.querySelector('a[href*="/store/custom-slug"]');
    expect(storeLink).toBeTruthy();
    expect(storeLink?.textContent?.trim()).toBe('Custom Store Name');
  });
});
