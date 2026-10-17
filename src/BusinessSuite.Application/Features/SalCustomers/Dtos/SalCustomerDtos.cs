namespace BusinessSuite.Application.Features.SalCustomers.Dtos;

public record SalCustomerListItemDto(
    string CustomerId,
    string CustomerCoName,
    string Address,
    string Phone1,
    string Phone2,
    string Phone3,
    bool Inactive);

public record SalCustomerDto(
    string CustomerId,
    string CustomerCoName,
    string CustomerShortName,
    string Phone1,
    string Phone2,
    string Phone3,
    string Email,
    string Url,
    string Address,
    bool Inactive);

public record SaveSalCustomerRequest(
    string CustomerCoName,
    string? CustomerShortName,
    string? Phone1,
    string? Phone2,
    string? Phone3,
    string? Email,
    string? Url,
    string Address,
    bool Inactive);
