namespace BusinessSuite.Domain.Entities;

/// <summary>
/// Legacy admin user row from tblAdmUser (existing database table).
/// Types match SQL: CompanyID tinyint, LocationID smallint.
/// </summary>
public class AdmUser
{
    public byte CompanyId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public bool Administrator { get; set; }
    public short LocationId { get; set; }
}
