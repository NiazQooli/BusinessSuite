using BusinessSuite.Application.Common.Interfaces;
using BusinessSuite.Application.Features.SalesInvoices.Dtos;
using BusinessSuite.Domain.Entities;
using BusinessSuite.Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusinessSuite.Application.Features.SalesInvoices.Commands;

public record SaveSalesInvoiceCommand(
    byte CompanyId,
    string UserId,
    int? SalesEntryId,
    SaveSalesInvoiceRequest Request) : IRequest<SalesInvoiceDto>;

public class SaveSalesInvoiceCommandValidator : AbstractValidator<SaveSalesInvoiceCommand>
{
    public SaveSalesInvoiceCommandValidator()
    {
        RuleFor(x => x.Request.FinYear).NotEmpty().MaximumLength(7);
        RuleFor(x => x.Request.CustomerId).NotEmpty().MaximumLength(40);
        RuleFor(x => x.Request.Lines).NotEmpty().WithMessage("At least one line is required.");
        RuleForEach(x => x.Request.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.FgId).GreaterThan(0);
            line.RuleFor(l => l.NoOfPacks).GreaterThan(0);
            line.RuleFor(l => l.BasicRate).GreaterThan(0);
            line.RuleFor(l => l.VatPercentage)
                .Must(v => v == 0 || v == 20)
                .WithMessage("Standard VAT Rate is 20%.");
        });
    }
}

public class SaveSalesInvoiceCommandHandler : IRequestHandler<SaveSalesInvoiceCommand, SalesInvoiceDto>
{
    private const short DefaultStockLocationId = 3;
    private readonly IApplicationDbContext _context;
    private readonly ISender _sender;

    public SaveSalesInvoiceCommandHandler(IApplicationDbContext context, ISender sender)
    {
        _context = context;
        _sender = sender;
    }

    public async Task<SalesInvoiceDto> Handle(SaveSalesInvoiceCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;
        var isEdit = command.SalesEntryId.HasValue && command.SalesEntryId.Value > 0;
        var salesEntryId = command.SalesEntryId ?? 0;
        SalSalesEntry header;

        if (!isEdit)
        {
            var maxId = await _context.SalSalesEntries
                .Where(e =>
                    e.CompanyId == command.CompanyId
                    && e.FinYear == req.FinYear
                    && e.BillType == "SI"
                    && e.SalesEntryType == "BG")
                .Select(e => (int?)e.SalesEntryId)
                .MaxAsync(cancellationToken) ?? 0;

            salesEntryId = maxId + 1;

            header = new SalSalesEntry
            {
                CompanyId = command.CompanyId,
                FinYear = req.FinYear,
                BillType = "SI",
                SalesEntryType = "BG",
                SalesEntryId = salesEntryId,
                SalesEntryDate = req.SalesEntryDate.Date,
                VehicleRegNo = (req.VehicleRegNo ?? string.Empty).Trim().ToUpperInvariant(),
                CustomerId = req.CustomerId.Trim(),
                TotalExcStValue = req.TotalExcStValue,
                TotalVatValue = req.TotalVatValue,
                TotalIncStValue = req.NetAmount,
                NetAmount = req.NetAmount,
                TotalWeightInTons = req.TotalWeightInTons,
                FreightApplied = false,
                StockLocationId = DefaultStockLocationId,
                DeliveredTo = string.IsNullOrWhiteSpace(req.DeliveredTo) ? "N/A" : req.DeliveredTo.Trim(),
                LastModifiedBy = command.UserId,
                Checked = false,
                CheckedBy = "N/A",
                Authorized = false,
                AuthorizedBy = "N/A",
                SalesDiscounts = "N",
                Is19LtrInvoice = "N",
                Warehouse = string.Empty,
                Currency = string.Empty,
            };
            _context.SalSalesEntries.Add(header);
        }
        else
        {
            header = await _context.SalSalesEntries.FirstOrDefaultAsync(e =>
                e.CompanyId == command.CompanyId
                && e.FinYear == req.FinYear
                && e.BillType == "SI"
                && e.SalesEntryType == "BG"
                && e.SalesEntryId == salesEntryId, cancellationToken)
                ?? throw new NotFoundException("SalSalesEntry", salesEntryId);

            if (header.Authorized)
            {
                throw new DomainException("Posted sales invoice cannot be edited.");
            }

            header.SalesEntryDate = req.SalesEntryDate.Date;
            header.VehicleRegNo = (req.VehicleRegNo ?? string.Empty).Trim().ToUpperInvariant();
            header.CustomerId = req.CustomerId.Trim();
            header.TotalExcStValue = req.TotalExcStValue;
            header.TotalVatValue = req.TotalVatValue;
            header.TotalIncStValue = req.NetAmount;
            header.NetAmount = req.NetAmount;
            header.TotalWeightInTons = req.TotalWeightInTons;
            header.DeliveredTo = string.IsNullOrWhiteSpace(req.DeliveredTo) ? "N/A" : req.DeliveredTo.Trim();
            header.LastModifiedBy = command.UserId;
            header.Checked = false;
            header.CheckedBy = "N/A";
            header.CheckedDate = null;

            var existingLines = await _context.SalSalesEntryDetails
                .Where(d =>
                    d.CompanyId == command.CompanyId
                    && d.FinYear == req.FinYear
                    && d.BillType == "SI"
                    && d.SalesEntryType == "BG"
                    && d.SalesEntryId == salesEntryId)
                .ToListAsync(cancellationToken);

            _context.SalSalesEntryDetails.RemoveRange(existingLines);
        }

        foreach (var line in req.Lines.Where(l => l.FgId > 0))
        {
            _context.SalSalesEntryDetails.Add(new SalSalesEntryDetail
            {
                CompanyId = command.CompanyId,
                FinYear = req.FinYear,
                BillType = "SI",
                SalesEntryType = "BG",
                SalesEntryId = salesEntryId,
                FgId = line.FgId,
                NoOfPacks = line.NoOfPacks,
                Weight = line.Weight,
                BasicRate = line.BasicRate,
                SpecialDiscount = line.SpecialDiscount,
                ExcStRate = line.ExcStRate,
                IncStValue = line.IncStValue,
                StRate = line.StRate,
                StAmount = line.StAmount,
                StAmount17 = line.StAmount17,
                StAmount03 = line.StAmount03,
                ExcStValue = line.ExcStValue,
                NetRate = line.NetRate,
                EmptyCartons = line.EmptyCartons,
                FreightCharges = line.FreightCharges,
                SoapCharges = line.SoapCharges,
                DistributorMargin = line.DistributorMargin,
                RetailerMargin = line.RetailerMargin,
                LeakageRate = line.LeakageRate,
                Discount1Percentage = line.Discount1Percentage,
                Discount1PercentageValue = line.Discount1PercentageValue,
                Discount2PerPack = line.Discount2PerPack,
                Discount2PerPackValue = line.Discount2PerPackValue,
                VatPercentage = line.VatPercentage,
                VatValue = line.VatValue,
            });
        }

        var cust = await _context.SalCustomerInfos.AsNoTracking()
            .FirstOrDefaultAsync(c =>
                c.CompanyId == command.CompanyId && c.CustomerId == req.CustomerId.Trim(), cancellationToken);

        if (cust?.IsDistShop == "Y")
        {
            header.Checked = true;
            header.CheckedBy = command.UserId;
            header.CheckedDate = DateTime.Now;
            header.Authorized = true;
            header.AuthorizedBy = command.UserId;
            header.AuthorizedDate = DateTime.Now;
        }

        await _context.SaveChangesAsync(cancellationToken);

        var saved = await _sender.Send(
            new Queries.GetSalesInvoiceQuery(command.CompanyId, req.FinYear, salesEntryId),
            cancellationToken);

        return saved ?? throw new DomainException("Sales invoice was saved but could not be reloaded.");
    }
}

public record DeleteSalesInvoiceCommand(byte CompanyId, string FinYear, int SalesEntryId, string UserId)
    : IRequest;

public class DeleteSalesInvoiceCommandHandler : IRequestHandler<DeleteSalesInvoiceCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteSalesInvoiceCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(DeleteSalesInvoiceCommand request, CancellationToken cancellationToken)
    {
        var header = await _context.SalSalesEntries.FirstOrDefaultAsync(e =>
            e.CompanyId == request.CompanyId
            && e.FinYear == request.FinYear
            && e.BillType == "SI"
            && e.SalesEntryType == "BG"
            && e.SalesEntryId == request.SalesEntryId, cancellationToken)
            ?? throw new NotFoundException("SalSalesEntry", request.SalesEntryId);

        if (header.Authorized)
        {
            throw new DomainException("Posted sales invoice cannot be deleted.");
        }

        // Delete detail rows first (FK_tblSalSalesEntryDetail_tblSalSalesEntry).
        // Must SaveChanges before removing header — EF has no configured cascade
        // relationship, so a single batch can delete the parent first and fail.
        var lines = await _context.SalSalesEntryDetails
            .Where(d =>
                d.CompanyId == header.CompanyId
                && d.FinYear == header.FinYear
                && d.BillType == header.BillType
                && d.SalesEntryType == header.SalesEntryType
                && d.SalesEntryId == header.SalesEntryId)
            .ToListAsync(cancellationToken);

        if (lines.Count > 0)
        {
            _context.SalSalesEntryDetails.RemoveRange(lines);
            await _context.SaveChangesAsync(cancellationToken);
        }

        _context.SalSalesEntries.Remove(header);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public record CheckSalesInvoiceCommand(byte CompanyId, string FinYear, int SalesEntryId, string UserId)
    : IRequest;

public class CheckSalesInvoiceCommandHandler : IRequestHandler<CheckSalesInvoiceCommand>
{
    private readonly IApplicationDbContext _context;

    public CheckSalesInvoiceCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(CheckSalesInvoiceCommand request, CancellationToken cancellationToken)
    {
        var header = await _context.SalSalesEntries.FirstOrDefaultAsync(e =>
            e.CompanyId == request.CompanyId
            && e.FinYear == request.FinYear
            && e.BillType == "SI"
            && e.SalesEntryType == "BG"
            && e.SalesEntryId == request.SalesEntryId, cancellationToken)
            ?? throw new NotFoundException("SalSalesEntry", request.SalesEntryId);

        header.Checked = true;
        header.CheckedBy = request.UserId;
        header.CheckedDate = DateTime.Now;
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public record PostSalesInvoiceCommand(byte CompanyId, string FinYear, int SalesEntryId, string UserId)
    : IRequest;

public class PostSalesInvoiceCommandHandler : IRequestHandler<PostSalesInvoiceCommand>
{
    private readonly IApplicationDbContext _context;

    public PostSalesInvoiceCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(PostSalesInvoiceCommand request, CancellationToken cancellationToken)
    {
        var header = await _context.SalSalesEntries.FirstOrDefaultAsync(e =>
            e.CompanyId == request.CompanyId
            && e.FinYear == request.FinYear
            && e.BillType == "SI"
            && e.SalesEntryType == "BG"
            && e.SalesEntryId == request.SalesEntryId, cancellationToken)
            ?? throw new NotFoundException("SalSalesEntry", request.SalesEntryId);

        if (header.Authorized)
        {
            throw new DomainException($"Sales Invoice already posted by: {header.AuthorizedBy}.");
        }

        // Stock ledger + accounts voucher remain as later migration steps (same VB PostingProcess).
        header.Authorized = true;
        header.AuthorizedBy = request.UserId;
        header.AuthorizedDate = DateTime.Now;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
