export interface ApiResponse<T> {
  data: T | null;
  error: { code: string; name: string } | null;
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
