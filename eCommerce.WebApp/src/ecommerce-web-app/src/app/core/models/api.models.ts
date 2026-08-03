// Shared response contracts returned by the API Gateway.
export interface ApiError {
  code: string;
  name: string;
}

export interface ApiResponse<T> {
  data: T | null;
  error: ApiError | null;
}

export interface PagedList<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface PageQuery {
  page?: number;
  pageSize?: number;
}
