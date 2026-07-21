export interface Customer {
  id: string;
  name: string;
  email: string;
  phone?: string | null;
  companyName?: string | null;
  isActive: boolean;
  createdAtUtc: string;
}

export interface PaginatedList<T> {
  items: T[];
  pageNumber: number;
  totalPages: number;
  totalCount: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface CreateCustomerRequest {
  name: string;
  email: string;
  phone?: string | null;
  companyName?: string | null;
}

export interface UpdateCustomerRequest extends CreateCustomerRequest {
  id: string;
  isActive: boolean;
}
