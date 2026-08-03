export interface Product {
  id: string;
  name: string;
  description: string;
  price: number;
  currency: string;
  quantity: number;
  imageIds: string[];
  rating: number;
  reviewsCount: number;
}

export interface ProductReview {
  id: string;
  productId: string;
  userId: string;
  rating: number;
  comment: string;
  createdAtUtc: string;
}

export interface ProductUpsertRequest {
  name: string;
  description: string;
  price: number;
  currencyCode: string;
  quantity: number;
  imageIds?: string[] | null;
}
