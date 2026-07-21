namespace BusinessSuite.Application.Features.Customers.Dtos;

public record CustomerDto(
    Guid Id,
    string Name,
    string Email,
    string? Phone,
    string? CompanyName,
    bool IsActive,
    DateTime CreatedAtUtc);
