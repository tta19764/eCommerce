// Seller API contracts for marketplace seller applications, store pages, and reviews.
export type SellerStatus = 0 | 1 | 2 | 3;

export const SellerStatus = {
  PendingReview: 0 as const,
  Active: 1 as const,
  Rejected: 2 as const,
  Suspended: 3 as const,
};

export interface SellerResponse {
  id: string;
  ownerUserId: string;
  status: SellerStatus;
  rejectionReason: string | null;
  createdOnUtc: string;
  reviewedOnUtc: string | null;
}

export interface StoreResponse {
  id: string;
  sellerId: string;
  slug: string;
  name: string;
  description: string;
  countryCode: string;
  defaultCurrency: string;
  logoImageId: string | null;
  bannerImageId: string | null;
  averageRating: number;
  reviewCount: number;
}

export interface StoreReviewResponse {
  id: string;
  customerUserId: string;
  sellerOrderId: string;
  rating: number;
  comment: string;
  createdOnUtc: string;
}

export interface CreateSellerApplicationRequest {
  slug: string;
  name: string;
  description: string;
  countryCode: string;
  defaultCurrency: string;
}

export interface CreateStoreReviewRequest {
  sellerOrderId: string;
  rating: number;
  comment: string;
}

export interface RejectSellerRequest {
  reason: string;
}

export interface PendingSellerApplicationResponse {
  sellerId: string;
  status: SellerStatus;
  applicant: {
    userId: string;
    fullName: string;
    email: string;
    found: boolean;
  };
  store: {
    storeId: string;
    slug: string;
    name: string;
    description: string;
    countryCode: string;
    defaultCurrency: string;
    logoImageId: string | null;
    bannerImageId: string | null;
  };
  submittedOnUtc: string;
}
