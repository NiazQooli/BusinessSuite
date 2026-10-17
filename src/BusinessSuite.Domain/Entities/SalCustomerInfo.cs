namespace BusinessSuite.Domain.Entities;

/// <summary>Legacy customer master — tblSalCustomerInfo (same table Sales Invoice uses).</summary>
public class SalCustomerInfo
{
    public byte CompanyId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerCoName { get; set; } = string.Empty;
    public string CustomerShortName { get; set; } = string.Empty;
    public string Phone1 { get; set; } = "N/A";
    public string Phone2 { get; set; } = "N/A";
    public string Phone3 { get; set; } = "N/A";
    public string Email { get; set; } = "N/A";
    public string Url { get; set; } = "N/A";
    public string Address { get; set; } = "N/A";
    public bool Inactive { get; set; }
    public string? DiscCategory { get; set; }
    public string? IsDistShop { get; set; }

    public void ApplyEdit(
        string customerCoName,
        string customerShortName,
        string phone1,
        string phone2,
        string phone3,
        string email,
        string url,
        string address,
        bool inactive)
    {
        CustomerCoName = customerCoName.Trim();
        CustomerShortName = customerShortName.Trim();
        Phone1 = ToNa(phone1);
        Phone2 = ToNa(phone2);
        Phone3 = ToNa(phone3);
        Email = ToNa(email);
        Url = ToNa(url);
        Address = ToNa(address);
        Inactive = inactive;
    }

    public static string ToNa(string? value)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        return string.IsNullOrWhiteSpace(trimmed) ? "N/A" : trimmed;
    }
}
