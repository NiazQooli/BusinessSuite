using BusinessSuite.Application.Common.Interfaces;
using BusinessSuite.Application.Common.Models;
using BusinessSuite.Application.Features.Customers.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusinessSuite.Application.Features.Customers.Queries.GetCustomers;

public record GetCustomersQuery(int PageNumber = 1, int PageSize = 10, string? Search = null)
    : IRequest<PaginatedList<CustomerDto>>;

public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, PaginatedList<CustomerDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCustomersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<CustomerDto>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 100 ? 10 : request.PageSize;

        var query = _context.Customers
            .AsNoTracking()
            .Where(c => !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(c =>
                c.Name.ToLower().Contains(search) ||
                c.Email.ToLower().Contains(search) ||
                (c.CompanyName != null && c.CompanyName.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(c => c.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CustomerDto(
                c.Id,
                c.Name,
                c.Email,
                c.Phone,
                c.CompanyName,
                c.IsActive,
                c.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return new PaginatedList<CustomerDto>(items, totalCount, pageNumber, pageSize);
    }
}
