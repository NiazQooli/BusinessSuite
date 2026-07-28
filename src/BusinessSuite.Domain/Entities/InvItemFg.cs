namespace BusinessSuite.Domain.Entities;

/// <summary>Legacy finished goods — tblInvItemFG (lookup fields).</summary>
public class InvItemFg
{
    public byte CompanyId { get; set; }
    public int FgId { get; set; }
    public short BrandId { get; set; }
    public string FgName { get; set; } = string.Empty;
    public short UomId { get; set; }
    public decimal NoOfUnitsInPack { get; set; }
    public decimal UnitWeight { get; set; }
    public decimal ForTonsConversion { get; set; }
    public decimal StPercentage { get; set; }
    public decimal EmptyCartonPercentage { get; set; }
    public decimal DistributorMargin { get; set; }
    public decimal RetailerMargin { get; set; }
    public decimal SedPercentage { get; set; }
    public short SgId { get; set; }
    public bool Inactive { get; set; }
}

public class InvItemFgBrand
{
    public short BrandId { get; set; }
    public string BrandTitle { get; set; } = string.Empty;
}

public class InvItemFgUom
{
    public short UomId { get; set; }
    public string UomTitle { get; set; } = string.Empty;
}

public class SftFinancialYearCompany
{
    public byte CompanyId { get; set; }
    public string FinancialYear { get; set; } = string.Empty;
    public string IsLocked { get; set; } = "N";
}
