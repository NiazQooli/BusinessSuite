using System.Data.Common;
using BusinessSuite.Application.Common.Interfaces;
using BusinessSuite.Application.Features.SalCustomers.Dtos;
using BusinessSuite.Application.Features.SalCustomers.Queries;
using BusinessSuite.Domain.Entities;
using BusinessSuite.Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusinessSuite.Application.Features.SalCustomers.Commands;

public record CreateSalCustomerCommand(byte CompanyId, SaveSalCustomerRequest Data) : IRequest<SalCustomerDto>;

public class CreateSalCustomerCommandValidator : AbstractValidator<CreateSalCustomerCommand>
{
    public CreateSalCustomerCommandValidator()
    {
        RuleFor(x => x.Data.CustomerCoName)
            .NotEmpty().WithMessage("Customer Name cannot be empty.")
            .MaximumLength(300);

        RuleFor(x => x.Data.Address)
            .NotEmpty().WithMessage("Address cannot be empty.")
            .MaximumLength(300);

        RuleFor(x => x.Data.CustomerShortName).MaximumLength(50);
        RuleFor(x => x.Data.Phone1).MaximumLength(15);
        RuleFor(x => x.Data.Phone2).MaximumLength(15);
        RuleFor(x => x.Data.Phone3).MaximumLength(15);
        RuleFor(x => x.Data.Email).MaximumLength(75);
        RuleFor(x => x.Data.Url).MaximumLength(75);
    }
}

public class CreateSalCustomerCommandHandler : IRequestHandler<CreateSalCustomerCommand, SalCustomerDto>
{
    private readonly IApplicationDbContext _context;

    public CreateSalCustomerCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<SalCustomerDto> Handle(CreateSalCustomerCommand request, CancellationToken cancellationToken)
    {
        var customerId = await NextCustomerIdAsync(request.CompanyId, cancellationToken);
        var d = request.Data;

        var name = d.CustomerCoName.Trim();
        var shortName = (d.CustomerShortName ?? string.Empty).Trim();
        var phone1 = SalCustomerInfo.ToNa(d.Phone1);
        var phone2 = SalCustomerInfo.ToNa(d.Phone2);
        var phone3 = SalCustomerInfo.ToNa(d.Phone3);
        var email = SalCustomerInfo.ToNa(d.Email);
        var url = SalCustomerInfo.ToNa(d.Url);
        var address = SalCustomerInfo.ToNa(d.Address);
        var inactive = d.Inactive;

        // Full INSERT with VB defaults so NOT NULL legacy columns stay compatible.
        await _context.ExecuteSqlAsync($"""
            INSERT INTO tblSalCustomerInfo(
                CompanyID, CustomerID, CustomerCoName, CustomerShortName, CustomerCategory, CustomerZone, Area_ID,
                Sub_Area_ID, DiscCategory, IncentiveCategory, Phone1, Phone2, Phone3, FaxNo, Email, URL, Address,
                ContactPerson, NICNo, NationalTaxNo, SalesTaxNo, IntegDefAccID, SecurityDepAccID, Inactive, IsTaxesApplied,
                BottlesConsumption, BottlesConsumptionMonthly, CreditLimit, StartingDate, AccountInitiatorID, DeliveryDate,
                CustomerType, PaymentMode, CustRemarks, CustomerKind, DeliveryType,
                DeliveryDay_Mon, DeliveryDay_Tue, DeliveryDay_Wed, DeliveryDay_Thu, DeliveryDay_Fri, DeliveryDay_Sat, DeliveryDay_Sun,
                NoOfBottlesRegistered, DepositAmount, WaterChargesPerBottle, WaterChargesPerCoupon, TotalDepositAmount,
                TotalWaterChargesPerBottle, TotalWaterChargesPerCoupon, TapPrice, StandPrice, DispenserPrice, TapQty, StandQty, DispenserQty,
                TotalTapPrice, TotalStandPrice, TotalDispenserPrice, GrandTotalAmount,
                InventoryStatus_BottleAlHilal, InventoryStatus_TapAlHilal, InventoryStatus_StandAlHilal, InventoryStatus_DispenserAlHilal,
                InventoryStatus_BottleCustomer, InventoryStatus_TapCustomer, InventoryStatus_StandCustomer, InventoryStatus_DispenserCustomer,
                DiscCategory_BluWater, RateCategory_BluWater, Designation, Reference, IncomeTaxPercentage, CustomerStatus,
                CityName, BusinessStatus, CustProvince, IsDistShop, IsIncludeInTarget)
            VALUES(
                {request.CompanyId}, {customerId}, {name}, {shortName}, 'Distributor', 'NA', NULL,
                NULL, 'NORMAL', 'NA', {phone1}, {phone2}, {phone3}, 'N/A', {email}, {url}, {address},
                'N/A', 'N/A', 'N/A', 'N/A', NULL, NULL, {inactive}, 0,
                0, 0, 0, NULL, NULL, NULL,
                '[ None ]', '[ None ]', 'N/A', '[ None ]', '[ None ]',
                0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0,
                '[ None ]', '', 'N/A', 'N/A', '0.0', '',
                '', '', '', 'N', 0)
            """, cancellationToken);

        return await new GetSalCustomerByIdQueryHandler(_context)
            .Handle(new GetSalCustomerByIdQuery(request.CompanyId, customerId), cancellationToken);
    }

    private async Task<string> NextCustomerIdAsync(byte companyId, CancellationToken cancellationToken)
    {
        var ids = await _context.SalCustomerInfos.AsNoTracking()
            .Where(c => c.CompanyId == companyId)
            .Select(c => c.CustomerId)
            .ToListAsync(cancellationToken);

        var max = ids
            .Select(id => int.TryParse(id, out var n) ? n : 0)
            .Where(n => n is >= 1 and <= 90000)
            .DefaultIfEmpty(0)
            .Max();

        return (max + 1).ToString("D5");
    }
}

public record UpdateSalCustomerCommand(byte CompanyId, string CustomerId, SaveSalCustomerRequest Data)
    : IRequest<SalCustomerDto>;

public class UpdateSalCustomerCommandValidator : AbstractValidator<UpdateSalCustomerCommand>
{
    public UpdateSalCustomerCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty().MaximumLength(40);
        RuleFor(x => x.Data.CustomerCoName)
            .NotEmpty().WithMessage("Customer Name cannot be empty.")
            .MaximumLength(300);
        RuleFor(x => x.Data.Address)
            .NotEmpty().WithMessage("Address cannot be empty.")
            .MaximumLength(300);
    }
}

public class UpdateSalCustomerCommandHandler : IRequestHandler<UpdateSalCustomerCommand, SalCustomerDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateSalCustomerCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<SalCustomerDto> Handle(UpdateSalCustomerCommand request, CancellationToken cancellationToken)
    {
        var customerId = request.CustomerId.Trim();

        var customer = await _context.SalCustomerInfos
            .FirstOrDefaultAsync(
                c => c.CompanyId == request.CompanyId && c.CustomerId == customerId,
                cancellationToken)
            ?? throw new NotFoundException(nameof(SalCustomerInfo), customerId);

        var d = request.Data;
        customer.ApplyEdit(
            d.CustomerCoName,
            d.CustomerShortName ?? string.Empty,
            d.Phone1 ?? string.Empty,
            d.Phone2 ?? string.Empty,
            d.Phone3 ?? string.Empty,
            d.Email ?? string.Empty,
            d.Url ?? string.Empty,
            d.Address,
            d.Inactive);

        await _context.SaveChangesAsync(cancellationToken);

        return GetSalCustomerByIdQueryHandler.Map(customer);
    }
}

public record DeleteSalCustomerCommand(byte CompanyId, string CustomerId) : IRequest;

public class DeleteSalCustomerCommandHandler : IRequestHandler<DeleteSalCustomerCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteSalCustomerCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(DeleteSalCustomerCommand request, CancellationToken cancellationToken)
    {
        var customerId = request.CustomerId.Trim();

        var exists = await _context.SalCustomerInfos.AsNoTracking()
            .AnyAsync(c => c.CompanyId == request.CompanyId && c.CustomerId == customerId, cancellationToken);

        if (!exists)
        {
            throw new NotFoundException(nameof(SalCustomerInfo), customerId);
        }

        try
        {
            var rows = await _context.ExecuteSqlAsync($"""
                DELETE FROM tblSalCustomerInfo
                WHERE CompanyID = {request.CompanyId} AND CustomerID = {customerId}
                """, cancellationToken);

            if (rows == 0)
            {
                throw new NotFoundException(nameof(SalCustomerInfo), customerId);
            }
        }
        catch (DbException ex) when (IsForeignKeyViolation(ex))
        {
            throw new DomainException(
                "System is unable to perform this action because related record exist in database.");
        }
    }

    private static bool IsForeignKeyViolation(DbException ex)
    {
        // SQL Server FK / constraint numbers via reflection (avoid SQL package in Application).
        var numberProp = ex.GetType().GetProperty("Number");
        if (numberProp?.GetValue(ex) is int number && number is 547 or 2627)
        {
            return true;
        }

        return ex.Message.Contains("REFERENCE constraint", StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase);
    }
}
