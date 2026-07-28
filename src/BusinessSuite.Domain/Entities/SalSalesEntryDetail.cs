namespace BusinessSuite.Domain.Entities;

/// <summary>Legacy sales invoice line — tblSalSalesEntryDetail.</summary>
public class SalSalesEntryDetail
{
    public byte CompanyId { get; set; }
    public string FinYear { get; set; } = string.Empty;
    public string BillType { get; set; } = "SI";
    public string SalesEntryType { get; set; } = "BG";
    public int SalesEntryId { get; set; }
    public int FgId { get; set; }
    public decimal NoOfPacks { get; set; }
    public decimal Weight { get; set; }
    public decimal BasicRate { get; set; }
    public decimal SpecialDiscount { get; set; }
    public decimal ExcStRate { get; set; }
    public decimal IncStValue { get; set; }
    public decimal SedRate { get; set; }
    public decimal SedAmount { get; set; }
    public decimal StRate { get; set; }
    public decimal StAmount { get; set; }
    public decimal StAmount17 { get; set; }
    public decimal StAmount03 { get; set; }
    public decimal ExcStValue { get; set; }
    public decimal NetRate { get; set; }
    public decimal FreightCharges { get; set; }
    public decimal SoapCharges { get; set; }
    public decimal EmptyCartons { get; set; }
    public decimal DistributorMargin { get; set; }
    public decimal RetailerMargin { get; set; }
    public decimal LeakageRate { get; set; }
    public decimal Discount1Percentage { get; set; }
    public decimal Discount1PercentageValue { get; set; }
    public decimal Discount2PerPack { get; set; }
    public decimal Discount2PerPackValue { get; set; }
    public bool IsIncludeInIncentiveBulk { get; set; }
    public bool IsIncludeInIncentiveConsumer { get; set; }
    public bool IsIncludeInIncentiveRso { get; set; }
    public decimal VatPercentage { get; set; }
    public decimal VatValue { get; set; }
}
