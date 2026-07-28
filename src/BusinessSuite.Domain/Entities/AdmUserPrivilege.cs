namespace BusinessSuite.Domain.Entities;

/// <summary>
/// Legacy per-menu rights from tblAdmUserPrivilege (existing database table).
/// Composite key: CompanyID + UserID + MenuName.
/// Permission flags drive screen access later (CanView, CanAdd, CanEdit, …).
/// HasRights is the legacy semicolon string (e.g. 1;1;1;1;1;1;1;0;1).
/// </summary>
public class AdmUserPrivilege
{
    public byte CompanyId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string MenuName { get; set; } = string.Empty;
    public string? HasRights { get; set; }
    public bool CanAdd { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanPost { get; set; }
    public bool CanPrint { get; set; }
    public bool CanView { get; set; }
    public bool CanExport { get; set; }
    public bool CanUnpost { get; set; }
    public bool CanCheck { get; set; }
}
