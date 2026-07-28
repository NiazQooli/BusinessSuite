export interface SalesInvoiceContext {
  companyId: number;
  finYear: string;
  salesEntryType: string;
  billType: string;
}

export interface SalesInvoiceSearchParams {
  finYear: string;
  search?: string;
  searchField?: string;
  sortOn?: 'authorized' | 'checked';
  sortFlag?: boolean;
  periodStart?: string;
  periodEnd?: string;
  take?: number;
}

export interface SalesInvoiceListItem {
  salesEntryType: string;
  salesEntryId: number;
  salesEntryDate: string;
  vehicleRegNo: string | null;
  customerId: string | null;
  customerName: string | null;
  address: string | null;
  deliveredTo: string | null;
  netAmount: number;
  totalWeightInTons: number;
  totalExcStValue: number;
  totalVatValue: number;
  checked: boolean;
  authorized: boolean;
  distShop: string | null;
}

export interface SalesInvoiceLine {
  fgId: number | null;
  brandName: string;
  fgName: string;
  noOfPacks: number | null;
  weight: number | null;
  uom: string;
  basicRate: number | null;
  specialDiscount: number;
  excStRate: number;
  incStValue: number;
  stRate: number;
  stAmount: number;
  excStValue: number;
  netRate: number;
  emptyCartons: number;
  noOfUnitsInPack: number;
  unitWeight: number;
  forTonsConversion: number;
  emptyCartonPercentage: number;
  freightCharges: number;
  soapCharges: number;
  distributorMargin: number;
  retailerMargin: number;
  leakageRate: number;
  vatPercentage: number | null;
  vatValue: number;
  discount1Percentage: number;
  discount1PercentageValue: number;
  discount2PerPack: number;
  discount2PerPackValue: number;
  sgId: number;
  stAmount17: number;
  stAmount03: number;
}

export interface SalesInvoice {
  finYear: string;
  billType: string;
  salesEntryType: string;
  salesEntryId: number;
  salesEntryDate: string;
  vehicleRegNo: string;
  customerId: string | null;
  customerName: string | null;
  address: string | null;
  distShop: string | null;
  salesPromotion: boolean;
  deliveredTo: string;
  stockLocationId: number | null;
  totalExcStValue: number;
  totalVatValue: number;
  netAmount: number;
  totalWeightInTons: number;
  checked: boolean;
  authorized: boolean;
  lines: SalesInvoiceLine[];
}

export interface CustomerLookup {
  customerId: string;
  customerName: string;
  address: string | null;
  distShop: string | null;
}

export interface FinishedGoodLookup {
  fgId: number;
  brandName: string;
  fgName: string;
  uom: string;
  noOfUnitsInPack: number;
  unitWeight: number;
  forTonsConversion: number;
  stPercentage: number;
  emptyCartonPercentage: number;
  distributorMargin: number;
  retailerMargin: number;
  sedPercentage: number;
  sgId: number;
  vatPercentage: number;
  basicRate: number;
  specialDiscount: number;
  freightCharges: number;
  soapCharges: number;
  leakageRate: number;
}

export interface SaveSalesInvoiceRequest {
  finYear: string;
  salesEntryDate: string;
  vehicleRegNo: string | null;
  customerId: string;
  deliveredTo: string | null;
  totalExcStValue: number;
  totalVatValue: number;
  netAmount: number;
  totalWeightInTons: number;
  lines: Array<{
    fgId: number;
    noOfPacks: number;
    weight: number;
    basicRate: number;
    specialDiscount: number;
    excStRate: number;
    incStValue: number;
    stRate: number;
    stAmount: number;
    excStValue: number;
    netRate: number;
    emptyCartons: number;
    freightCharges: number;
    soapCharges: number;
    distributorMargin: number;
    retailerMargin: number;
    leakageRate: number;
    vatPercentage: number;
    vatValue: number;
    discount1Percentage: number;
    discount1PercentageValue: number;
    discount2PerPack: number;
    discount2PerPackValue: number;
    stAmount17: number;
    stAmount03: number;
  }>;
}
