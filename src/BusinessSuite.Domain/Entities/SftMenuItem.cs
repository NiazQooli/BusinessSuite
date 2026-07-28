namespace BusinessSuite.Domain.Entities;

/// <summary>
/// Legacy menu catalog from tblSftMenuItems (existing database table).
/// Composite key: CompanyID + MenuName.
/// Hierarchy: MainMenuName parent key; MenuLevel / MenuSrNo / MenuPriority for ordering.
/// MenuCaption is the display text; MenuName is the stable privilege key.
/// </summary>
public class SftMenuItem
{
    public byte CompanyId { get; set; }
    public string MenuName { get; set; } = string.Empty;
    public string MenuSrNo { get; set; } = string.Empty;
    public string? MainMenuName { get; set; }
    public string MenuCaption { get; set; } = string.Empty;
    public string? HasRights { get; set; }
    public byte? MenuLevel { get; set; }
    public string CompleteMenuName { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public bool IsReportActive { get; set; }
    public byte MenuPriority { get; set; }
    public bool InactiveMenu { get; set; }
}
