namespace BusinessSuite.Domain.Entities;

/// <summary>Legacy sales invoice header — tblSalSalesEntry (BG / SI).</summary>
public class SalSalesEntry
{
    public byte CompanyId { get; set; }
    public string FinYear { get; set; } = string.Empty;
    public string BillType { get; set; } = "SI";
    public string SalesEntryType { get; set; } = "BG";
    public int SalesEntryId { get; set; }
    public DateTime SalesEntryDate { get; set; }
    public int? OrderId { get; set; }
    public string VehicleRegNo { get; set; } = string.Empty;
    public string? CustomerId { get; set; }
    public decimal TotalExcStValue { get; set; }
    public decimal TotalVatValue { get; set; }
    public decimal TotalSedValue { get; set; }
    public decimal TotalStValue { get; set; }
    public decimal TotalStValue17 { get; set; }
    public decimal TotalStValue03 { get; set; }
    public decimal TotalIncStValue { get; set; }
    public decimal TotalDiscount1PercentageValue { get; set; }
    public decimal TotalDiscount2PerPackValue { get; set; }
    public decimal NetAmount { get; set; }
    public decimal TotalWeightInTons { get; set; }
    public decimal TotalBGheenCOilWeight { get; set; }
    public decimal TotalFreightCharges { get; set; }
    public decimal TotalSoapCharges { get; set; }
    public int TotalEmptyCartons { get; set; }
    public bool FreightApplied { get; set; }
    public string? CashAccId { get; set; }
    public short? StockLocationId { get; set; }
    public string DeliveredTo { get; set; } = string.Empty;
    public string LastModifiedBy { get; set; } = string.Empty;
    public bool Checked { get; set; }
    public string CheckedBy { get; set; } = "N/A";
    public DateTime? CheckedDate { get; set; }
    public bool Authorized { get; set; }
    public string AuthorizedBy { get; set; } = "N/A";
    public DateTime? AuthorizedDate { get; set; }
    public string? Station { get; set; }
    public string? VouType { get; set; }
    public string? VouNo { get; set; }
    public string? Station1 { get; set; }
    public string? VouType1 { get; set; }
    public string? VouNo1 { get; set; }
    public bool IsIncludeInIncentiveBulk { get; set; }
    public bool IsIncludeInIncentiveConsumer { get; set; }
    public bool IsIncludeInIncentiveRso { get; set; }
    public byte CreditDays { get; set; }
    public decimal TotalCommission { get; set; }
    public decimal IncomeTaxPercentage { get; set; }
    public decimal IncomeTax { get; set; }
    public decimal IncomeTaxIncValue { get; set; }
    public DateTime? DespatchDate { get; set; }
    public string SalesDiscounts { get; set; } = "N";
    public string? CustCodeSalesProm { get; set; }
    public string Is19LtrInvoice { get; set; } = "N";
    public decimal SkStPercentage { get; set; }
    public string Warehouse { get; set; } = string.Empty;
    public decimal CarriageCharges { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal CurrencyRate { get; set; }
}
