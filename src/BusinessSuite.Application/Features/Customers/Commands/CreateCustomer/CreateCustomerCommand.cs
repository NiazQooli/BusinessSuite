using BusinessSuite.Application.Common.Interfaces;
using BusinessSuite.Application.Features.Customers.Dtos;
using BusinessSuite.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusinessSuite.Application.Features.Customers.Commands.CreateCustomer;

public record CreateCustomerCommand(
    string Name,
    string Email,
    string? Phone,
    string? CompanyName) : IRequest<CustomerDto>;

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be valid.")
            .MaximumLength(256);

        RuleFor(x => x.Phone).MaximumLength(50);
        RuleFor(x => x.CompanyName).MaximumLength(200);
    }
}

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CustomerDto>
{
    private readonly IApplicationDbContext _context;

    public CreateCustomerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var emailExists = await _context.Customers
            .AnyAsync(c => c.Email == request.Email.Trim().ToLowerInvariant() && !c.IsDeleted, cancellationToken);

        if (emailExists)
        {
            throw new Common.Exceptions.ValidationException(
                new[] { new FluentValidation.Results.ValidationFailure(nameof(request.Email), "A customer with this email already exists.") });
        }

        var customer = Customer.Create(request.Name, request.Email, request.Phone, request.CompanyName);

        _context.Customers.Add(customer);
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
