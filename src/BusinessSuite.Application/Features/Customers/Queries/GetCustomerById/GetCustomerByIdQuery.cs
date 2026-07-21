using BusinessSuite.Application.Common.Interfaces;
using BusinessSuite.Application.Features.Customers.Dtos;
using BusinessSuite.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusinessSuite.Application.Features.Customers.Queries.GetCustomerById;

public record GetCustomerByIdQuery(Guid Id) : IRequest<CustomerDto>;

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDto>
{
    private readonly IApplicationDbContext _context;

    public GetCustomerByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerDto> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .Where(c => c.Id == request.Id && !c.IsDeleted)
            .Select(c => new CustomerDto(
                c.Id,
                c.Name,
                c.Email,
                c.Phone,
                c.CompanyName,
                c.IsActive,
                c.CreatedAtUtc))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Customer), request.Id);

        return customer;
    }
}
