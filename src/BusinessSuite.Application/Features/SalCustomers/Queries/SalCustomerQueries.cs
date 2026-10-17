using BusinessSuite.Application.Common.Interfaces;
using BusinessSuite.Application.Common.Models;
using BusinessSuite.Application.Features.SalCustomers.Dtos;
using BusinessSuite.Domain.Entities;
using BusinessSuite.Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusinessSuite.Application.Features.SalCustomers.Queries;

public record SearchSalCustomersQuery(
    byte CompanyId,
    int PageNumber = 1,
    int PageSize = 500,
    string? Search = null) : IRequest<PaginatedList<SalCustomerListItemDto>>;

public class SearchSalCustomersQueryValidator : AbstractValidator<SearchSalCustomersQuery>
{
    public SearchSalCustomersQueryValidator()
    {
        RuleFor(x => x.CompanyId).GreaterThan((byte)0);
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 1000);
    }
}

public class SearchSalCustomersQueryHandler
    : IRequestHandler<SearchSalCustomersQuery, PaginatedList<SalCustomerListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public SearchSalCustomersQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<PaginatedList<SalCustomerListItemDto>> Handle(
        SearchSalCustomersQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.SalCustomerInfos.AsNoTracking()
            .Where(c => c.CompanyId == request.CompanyId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLowerInvariant();
            query = query.Where(c =>
                c.CustomerId.ToLower().Contains(term)
                || c.CustomerCoName.ToLower().Contains(term)
                || c.Address.ToLower().Contains(term)
                || c.Phone1.ToLower().Contains(term)
                || c.Phone2.ToLower().Contains(term)
                || c.Phone3.ToLower().Contains(term));
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(c => c.CustomerId)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new SalCustomerListItemDto(
                c.CustomerId,
                c.CustomerCoName,
                c.Address,
                c.Phone1,
                c.Phone2,
                c.Phone3,
                c.Inactive))
            .ToListAsync(cancellationToken);

        // Present N/A placeholders as blank in the UI (VB SetNAFields reverse).
        items = items
            .Select(c => c with
            {
                Address = GetSalCustomerByIdQueryHandler.Display(c.Address),
                Phone1 = GetSalCustomerByIdQueryHandler.Display(c.Phone1),
                Phone2 = GetSalCustomerByIdQueryHandler.Display(c.Phone2),
                Phone3 = GetSalCustomerByIdQueryHandler.Display(c.Phone3),
            })
            .ToList();

        return new PaginatedList<SalCustomerListItemDto>(items, total, request.PageNumber, request.PageSize);
    }
}

public record GetSalCustomerByIdQuery(byte CompanyId, string CustomerId) : IRequest<SalCustomerDto>;

public class GetSalCustomerByIdQueryHandler : IRequestHandler<GetSalCustomerByIdQuery, SalCustomerDto>
{
    private readonly IApplicationDbContext _context;

    public GetSalCustomerByIdQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<SalCustomerDto> Handle(GetSalCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customerId = request.CustomerId.Trim();

        var customer = await _context.SalCustomerInfos.AsNoTracking()
            .FirstOrDefaultAsync(
                c => c.CompanyId == request.CompanyId && c.CustomerId == customerId,
                cancellationToken)
            ?? throw new NotFoundException(nameof(SalCustomerInfo), customerId);

        return Map(customer);
    }

    internal static SalCustomerDto Map(SalCustomerInfo c) => new(
        c.CustomerId,
        c.CustomerCoName,
        Display(c.CustomerShortName),
        Display(c.Phone1),
        Display(c.Phone2),
        Display(c.Phone3),
        Display(c.Email),
        Display(c.Url),
        Display(c.Address),
        c.Inactive);

    internal static string Display(string? value)
        => string.IsNullOrWhiteSpace(value)
           || string.Equals(value, "N/A", StringComparison.OrdinalIgnoreCase)
            ? string.Empty
            : value;
}
