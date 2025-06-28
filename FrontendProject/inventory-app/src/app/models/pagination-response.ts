export interface PaginationResponse<T> {
  data: T[];
  pagination: {
    totalRecords: number;
    page: number;
    pageSize: number;
    totalPages: number;
  };
}
