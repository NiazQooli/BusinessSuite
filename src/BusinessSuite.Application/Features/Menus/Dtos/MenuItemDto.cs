namespace BusinessSuite.Application.Features.Menus.Dtos;

public record MenuItemDto(
    byte CompanyId,
    string MenuName,
    string MenuSrNo,
    string? MainMenuName,
    string MenuCaption,
    string? HasRights,
    byte? MenuLevel,
    string CompleteMenuName,
    string ModuleName,
    bool IsReportActive,
    byte MenuPriority,
    bool InactiveMenu);
