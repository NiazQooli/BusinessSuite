export interface SalCustomerListItem {
  customerId: string;
  customerCoName: string;
  address: string;
  phone1: string;
  phone2: string;
  phone3: string;
  inactive: boolean;
}

export interface SalCustomer {
  customerId: string;
  customerCoName: string;
  customerShortName: string;
  phone1: string;
  phone2: string;
  phone3: string;
  email: string;
  url: string;
  address: string;
  inactive: boolean;
}

export interface SaveSalCustomerRequest {
  customerCoName: string;
  customerShortName?: string | null;
  phone1?: string | null;
  phone2?: string | null;
  phone3?: string | null;
  email?: string | null;
  url?: string | null;
  address: string;
  inactive: boolean;
}

export interface PaginatedSalCustomers {
  items: SalCustomerListItem[];
  pageNumber: number;
  totalPages: number;
  totalCount: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
