namespace BusinessSuite.Application.Features.Auth.Dtos;

public record LoginResponseDto(
    string AccessToken,
    DateTime ExpiresAtUtc,
    string UserId,
    int CompanyId,
    string UserName,
    string FullName);
