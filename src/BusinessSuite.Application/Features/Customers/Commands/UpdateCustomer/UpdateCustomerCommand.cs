using BusinessSuite.Application.Common.Interfaces;
using BusinessSuite.Application.Features.Customers.Dtos;
using BusinessSuite.Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusinessSuite.Application.Features.Customers.Commands.UpdateCustomer;

public record UpdateCustomerCommand(
    Guid Id,
    string Name,
    string Email,
    string? Phone,
    string? CompanyName,
    bool IsActive) : IRequest<CustomerDto>;

public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Phone).MaximumLength(50);
        RuleFor(x => x.CompanyName).MaximumLength(200);
    }
}

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, CustomerDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateCustomerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerDto> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.Id && !c.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Customer), request.Id);

        var emailTaken = await _context.Customers
            .AnyAsync(c => c.Id != request.Id
                           && c.Email == request.Email.Trim().ToLowerInvariant()
                           && !c.IsDeleted, cancellationToken);

        if (emailTaken)
        {
            throw new Common.Exceptions.ValidationException(
                new[] { new FluentValidation.Results.ValidationFailure(nameof(request.Email), "A customer with this email already exists.") });
        }

        customer.Update(request.Name, request.Email, request.Phone, request.CompanyName, request.IsActive);
        await _context.SaveChangesAsync(cancellationToken);

        return new CustomerDto(
            customer.Id,
            customer.Name,
            customer.Email,
            customer.Phone,
            customer.CompanyName,
            customer.IsActive,
            customer.CreatedAtUtc);
    }
}
