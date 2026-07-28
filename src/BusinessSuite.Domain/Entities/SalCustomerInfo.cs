namespace BusinessSuite.Domain.Entities;

/// <summary>Legacy customer master — tblSalCustomerInfo.</summary>
public class SalCustomerInfo
{
    public byte CompanyId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerCoName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? DiscCategory { get; set; }
    public bool Inactive { get; set; }
    public string? IsDistShop { get; set; }
}
