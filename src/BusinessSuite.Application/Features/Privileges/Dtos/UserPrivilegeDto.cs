namespace BusinessSuite.Application.Features.Privileges.Dtos;

public record UserPrivilegeDto(
    byte CompanyId,
    string UserId,
    string MenuName,
    string? HasRights,
    bool CanAdd,
    bool CanEdit,
    bool CanDelete,
    bool CanPost,
    bool CanPrint,
    bool CanView,
    bool CanExport,
    bool CanUnpost,
    bool CanCheck);
