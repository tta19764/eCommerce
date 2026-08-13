import '@angular/compiler';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { ImagesApiClient } from '../../../../core/api/images-api';
import { ProductsApiClient } from '../../../../core/api/products-api';
import { PagedList } from '../../../../core/models/api-model';
import { Product } from '../../../../core/models/product-model';
import { AdminProductsPage } from './admin-products-page';

describe('AdminProductsPage', () => {
  let productsApiMock: Partial<ProductsApiClient>;
  let imagesApiMock: Partial<ImagesApiClient>;

  const mockProduct: Product = {
    id: 'prod-101',
    name: 'Mechanical Gaming Keyboard',
    description: 'RGB mechanical keyboard with blue switches',
    price: 99.99,
    currency: 'USD',
    quantity: 15,
    sellerId: 'seller-1',
    categoryId: 'cat-1',
    productType: 'Physical',
    imageIds: ['img-1'],
    displayImageId: 'img-1',
    rating: 5,
    reviewsCount: 1,
  };

  const mockPageResult: PagedList<Product> = {
    items: [mockProduct],
    totalCount: 1,
    page: 1,
    pageSize: 50,
  };

  beforeEach(() => {
    productsApiMock = {
      getPage: vi.fn().mockReturnValue(of(mockPageResult)),
      getCategories: vi.fn().mockReturnValue(of([])),
      getTypes: vi.fn().mockReturnValue(of([])),
      create: vi.fn().mockReturnValue(of(mockProduct)),
      update: vi.fn().mockReturnValue(of(mockProduct)),
      delete: vi.fn().mockReturnValue(of(undefined)),
    };

    imagesApiMock = {
      contentUrl: vi.fn().mockReturnValue('http://localhost:5000/images/img-1'),
      upload: vi.fn().mockReturnValue(of({ id: 'img-1' })),
    };

    TestBed.configureTestingModule({
      imports: [AdminProductsPage],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: ProductsApiClient, useValue: productsApiMock },
        { provide: ImagesApiClient, useValue: imagesApiMock },
      ],
    });
  });

  it('loads product inventory on initialization', () => {
    const fixture = TestBed.createComponent(AdminProductsPage);
    fixture.detectChanges();
    const component = fixture.componentInstance;

    expect(productsApiMock.getPage).toHaveBeenCalledWith({ page: 1, pageSize: 50 });
    expect((component as any).products().length).toBe(1);
    expect((component as any).products()[0].name).toBe('Mechanical Gaming Keyboard');
  });

  it('prompts delete modal and sets deletingProduct signal', () => {
    const fixture = TestBed.createComponent(AdminProductsPage);
    fixture.detectChanges();
    const component = fixture.componentInstance;

    (component as any).promptDelete(mockProduct);
    expect((component as any).deletingProduct()).toEqual(mockProduct);
  });

  it('cancels delete and clears deletingProduct signal', () => {
    const fixture = TestBed.createComponent(AdminProductsPage);
    fixture.detectChanges();
    const component = fixture.componentInstance;

    (component as any).promptDelete(mockProduct);
    expect((component as any).deletingProduct()).toEqual(mockProduct);

    (component as any).cancelDelete();
    expect((component as any).deletingProduct()).toBeNull();
  });

  it('confirmDelete calls productsApi.delete and reloads inventory on success', () => {
    const fixture = TestBed.createComponent(AdminProductsPage);
    fixture.detectChanges();
    const component = fixture.componentInstance;

    (component as any).promptDelete(mockProduct);
    (component as any).confirmDelete();

    expect(productsApiMock.delete).toHaveBeenCalledWith('prod-101');
    expect((component as any).deletingProduct()).toBeNull();
    expect((component as any).success()).toBe('Product "Mechanical Gaming Keyboard" deleted successfully.');
  });

  it('confirmDelete handles error gracefully and leaves modal open for retry', () => {
    productsApiMock.delete = vi.fn().mockReturnValue(throwError(() => new Error('Delete failed')));

    const fixture = TestBed.createComponent(AdminProductsPage);
    fixture.detectChanges();
    const component = fixture.componentInstance;

    (component as any).promptDelete(mockProduct);
    (component as any).confirmDelete();

    expect(productsApiMock.delete).toHaveBeenCalledWith('prod-101');
    expect((component as any).deleting()).toBe(false);
    expect((component as any).error()).toBe('Delete failed');
  });

  it('closes delete modal on window Escape key press', () => {
    const fixture = TestBed.createComponent(AdminProductsPage);
    fixture.detectChanges();
    const component = fixture.componentInstance;

    (component as any).promptDelete(mockProduct);
    expect((component as any).deletingProduct()).toEqual(mockProduct);

    window.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape' }));
    expect((component as any).deletingProduct()).toBeNull();
  });
});
