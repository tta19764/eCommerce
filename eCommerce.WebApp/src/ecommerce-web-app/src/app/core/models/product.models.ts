// Product API contracts used by the catalog and product administration pages.
export type ProductType =
  'Physical' | 'DigitalDownload' | 'LicenseKey' | 'Service' | 'Subscription' | 'Bundle';

export type ProductSortBy = 'Default' | 'Name' | 'Price' | 'Rating';

export interface Product {
  id: string;
  name: string;
  description: string;
  price: number;
  currency: string;
  quantity: number;
  sellerId: string;
  categoryId: string;
  productType: ProductType;
  imageIds: string[];
  displayImageId: string | null;
  rating: number;
  reviewsCount: number;
}

export interface ProductCategory {
  id: string;
  name: string;
  slug: string;
  parentCategoryId: string | null;
  depth: number;
  subcategories: ProductCategory[];
}

export interface CreateCategoryRequest {
  name: string;
  slug?: string;
  parentCategoryId?: string | null;
}

export interface FlatCategoryOption {
  id: string;
  name: string;
  slug: string;
  parentCategoryId: string | null;
  depth: number;
  fullPath: string;
  indentedName: string;
}

export interface ProductTypeOption {
  value: ProductType;
  label: string;
  description: string;
}

export interface ProductSearchQuery {
  page?: number;
  pageSize?: number;
  query?: string | null;
  categoryId?: string | null;
  includeSubcategories?: boolean;
  productType?: ProductType | null;
  sellerId?: string | null;
  minPrice?: number | null;
  maxPrice?: number | null;
  minRating?: number | null;
  inStock?: boolean | null;
  sortBy?: ProductSortBy;
  sortDescending?: boolean;
}

export interface ProductReview {
  id: string;
  productId: string;
  userId: string;
  rating: number;
  comment: string;
  createdAtUtc: string;
}

export interface CreateProductRequest {
  name: string;
  description: string;
  price: number;
  currencyCode: string;
  quantity: number;
  sellerId: string;
  categoryId: string;
  productType: ProductType;
  imageIds?: string[] | null;
  displayImageId?: string | null;
}

export type UpdateProductRequest = CreateProductRequest;
