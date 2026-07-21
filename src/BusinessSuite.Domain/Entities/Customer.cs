using BusinessSuite.Domain.Common;

namespace BusinessSuite.Domain.Entities;

public class Customer : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? CompanyName { get; set; }
    public bool IsActive { get; set; } = true;

    public static Customer Create(string name, string email, string? phone = null, string? companyName = null)
    {
        return new Customer
        {
            Name = name.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            Phone = phone?.Trim(),
            CompanyName = companyName?.Trim(),
            IsActive = true
        };
    }

    public void Update(string name, string email, string? phone, string? companyName, bool isActive)
    {
        Name = name.Trim();
        Email = email.Trim().ToLowerInvariant();
        Phone = phone?.Trim();
        CompanyName = companyName?.Trim();
        IsActive = isActive;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
