using BusinessSuite.Application.Common.Interfaces;
using BusinessSuite.Application.Features.SalesInvoices.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusinessSuite.Application.Features.SalesInvoices.Queries;

public record GetSalesInvoiceContextQuery(byte CompanyId) : IRequest<SalesInvoiceContextDto>;

public class GetSalesInvoiceContextQueryHandler
    : IRequestHandler<GetSalesInvoiceContextQuery, SalesInvoiceContextDto>
{
    private readonly IApplicationDbContext _context;

    public GetSalesInvoiceContextQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<SalesInvoiceContextDto> Handle(
        GetSalesInvoiceContextQuery request,
        CancellationToken cancellationToken)
    {
        var finYear = await _context.SftFinancialYearCompanies
            .AsNoTracking()
            .Where(y => y.CompanyId == request.CompanyId && y.IsLocked != "Y")
            .OrderByDescending(y => y.FinancialYear)
            .Select(y => y.FinancialYear)
            .FirstOrDefaultAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(finYear))
        {
            finYear = DateTime.Today.Year + "-" + (DateTime.Today.Year + 1).ToString()[2..];
        }

        return new SalesInvoiceContextDto(request.CompanyId, finYear, "BG", "SI");
    }
}

public record SearchSalesInvoicesQuery(
    byte CompanyId,
    string FinYear,
    string? Search = null,
    string? SearchField = null,
    string? SortOn = null,
    bool? SortFlag = null,
    DateTime? PeriodStart = null,
    DateTime? PeriodEnd = null,
    int Take = 50)
    : IRequest<IReadOnlyList<SalesInvoiceListItemDto>>;

public class SearchSalesInvoicesQueryHandler
    : IRequestHandler<SearchSalesInvoicesQuery, IReadOnlyList<SalesInvoiceListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public SearchSalesInvoicesQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<SalesInvoiceListItemDto>> Handle(
        SearchSalesInvoicesQuery request,
        CancellationToken cancellationToken)
    {
        var take = request.Take is < 1 or > 500 ? 50 : request.Take;
        var query =
            from se in _context.SalSalesEntries.AsNoTracking()
            join cust in _context.SalCustomerInfos.AsNoTracking()
                on new { se.CompanyId, CustomerId = se.CustomerId! }
                equals new { cust.CompanyId, cust.CustomerId }
                into custJoin
            from cust in custJoin.DefaultIfEmpty()
            where se.CompanyId == request.CompanyId
                  && se.FinYear == request.FinYear
                  && se.BillType == "SI"
                  && se.SalesEntryType == "BG"
                  && se.Is19LtrInvoice != "Y"
            select new { se, cust };

        if (request.PeriodStart.HasValue)
        {
            var start = request.PeriodStart.Value.Date;
            query = query.Where(x => x.se.SalesEntryDate >= start);
        }

        if (request.PeriodEnd.HasValue)
        {
            var end = request.PeriodEnd.Value.Date;
            query = query.Where(x => x.se.SalesEntryDate <= end);
        }

        var sortOn = request.SortOn?.Trim().ToLowerInvariant();
        if (request.SortFlag.HasValue && sortOn is "authorized" or "checked")
        {
            var flag = request.SortFlag.Value;
            query = sortOn == "authorized"
                ? query.Where(x => x.se.Authorized == flag)
                : query.Where(x => x.se.Checked == flag);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            var field = request.SearchField?.Trim().ToLowerInvariant() ?? "all";
            var like = $"%{EscapeLike(term)}%";
            var parsedDay = TryParseSearchDate(term);

            switch (field)
            {
                case "sitype":
                    query = query.Where(x => EF.Functions.Like(x.se.SalesEntryType, like));
                    break;
                case "sino":
                    query = query.Where(x => x.se.SalesEntryId.ToString().Contains(term));
                    break;
                case "sidate":
                    if (parsedDay.HasValue)
                    {
                        var day = parsedDay.Value;
                        query = query.Where(x => x.se.SalesEntryDate == day);
                    }
                    break;
                case "code":
                    query = query.Where(x =>
                        x.se.CustomerId != null && EF.Functions.Like(x.se.CustomerId, like));
                    break;
                case "customername":
                    query = query.Where(x =>
                        x.cust != null && EF.Functions.Like(x.cust.CustomerCoName, like));
                    break;
                case "deliveredto":
                    query = query.Where(x => EF.Functions.Like(x.se.DeliveredTo, like));
                    break;
                case "all":
                default:
                    query = query.Where(x =>
                        x.se.SalesEntryId.ToString().Contains(term)
                        || (x.se.CustomerId != null && EF.Functions.Like(x.se.CustomerId, like))
                        || (x.cust != null && EF.Functions.Like(x.cust.CustomerCoName, like))
                        || EF.Functions.Like(x.se.DeliveredTo, like)
                        || EF.Functions.Like(x.se.VehicleRegNo, like)
                        || EF.Functions.Like(x.se.SalesEntryType, like)
                        || (parsedDay.HasValue && x.se.SalesEntryDate == parsedDay.Value));
                    break;
            }
        }

        try
        {
            return await query
                .OrderByDescending(x => x.se.SalesEntryDate)
                .ThenByDescending(x => x.se.SalesEntryId)
                .Take(take)
                .Select(x => new SalesInvoiceListItemDto(
                    x.se.SalesEntryType,
                    x.se.SalesEntryId,
                    x.se.SalesEntryDate,
                    x.se.VehicleRegNo,
                    x.se.CustomerId,
                    x.cust != null ? x.cust.CustomerCoName : null,
                    x.cust != null ? x.cust.Address : null,
                    x.se.DeliveredTo,
                    x.se.NetAmount,
                    x.se.TotalWeightInTons,
                    x.se.TotalExcStValue,
                    x.se.TotalVatValue,
                    x.se.Checked,
                    x.se.Authorized,
                    x.cust != null && x.cust.IsDistShop == "Y" ? "DIST. SHOP" : "-"))
                .ToListAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Client canceled (e.g. Find filter changed) — return empty, not a server error.
            return Array.Empty<SalesInvoiceListItemDto>();
        }
    }

    private static string EscapeLike(string value) =>
        value
            .Replace("[", "[[]", StringComparison.Ordinal)
            .Replace("%", "[%]", StringComparison.Ordinal)
            .Replace("_", "[_]", StringComparison.Ordinal);

    /// <summary>
    /// Accepts VB-style dates: dd/MM/yyyy, dd-MM-yyyy, yyyy-MM-dd, and culture parse fallback.
    /// </summary>
    private static DateTime? TryParseSearchDate(string term)
    {
        var formats = new[]
        {
            "dd/MM/yyyy",
            "d/M/yyyy",
            "dd-MM-yyyy",
            "d-M-yyyy",
            "yyyy-MM-dd",
            "dd/MM/yy",
            "d/M/yy",
        };

        if (DateTime.TryParseExact(
                term,
                formats,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out var exact))
        {
            return exact.Date;
        }

        if (DateTime.TryParse(
                term,
                System.Globalization.CultureInfo.GetCultureInfo("en-GB"),
                System.Globalization.DateTimeStyles.None,
                out var uk))
        {
            return uk.Date;
        }

        return null;
    }
}

public record GetSalesInvoiceQuery(byte CompanyId, string FinYear, int SalesEntryId)
    : IRequest<SalesInvoiceDto?>;

public class GetSalesInvoiceQueryHandler : IRequestHandler<GetSalesInvoiceQuery, SalesInvoiceDto?>
{
    private readonly IApplicationDbContext _context;

    public GetSalesInvoiceQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<SalesInvoiceDto?> Handle(GetSalesInvoiceQuery request, CancellationToken cancellationToken)
    {
        var header = await _context.SalSalesEntries
            .AsNoTracking()
            .FirstOrDefaultAsync(e =>
                e.CompanyId == request.CompanyId
                && e.FinYear == request.FinYear
                && e.BillType == "SI"
                && e.SalesEntryType == "BG"
                && e.SalesEntryId == request.SalesEntryId, cancellationToken);

        if (header is null)
        {
            return null;
        }

        string? customerName = null;
        string? address = null;
        string? distShop = null;

        if (!string.IsNullOrWhiteSpace(header.CustomerId))
        {
            var cust = await _context.SalCustomerInfos.AsNoTracking()
                .FirstOrDefaultAsync(c =>
                    c.CompanyId == request.CompanyId && c.CustomerId == header.CustomerId, cancellationToken);
            if (cust is not null)
            {
                customerName = cust.CustomerCoName;
                address = cust.Address;
                distShop = cust.IsDistShop;
            }
        }

        var lines = await (
            from d in _context.SalSalesEntryDetails.AsNoTracking()
            join fg in _context.InvItemFgs.AsNoTracking()
                on new { d.CompanyId, d.FgId } equals new { fg.CompanyId, fg.FgId }
            join brand in _context.InvItemFgBrands.AsNoTracking()
                on fg.BrandId equals brand.BrandId
            join uom in _context.InvItemFgUoms.AsNoTracking()
                on fg.UomId equals uom.UomId
            where d.CompanyId == request.CompanyId
                  && d.FinYear == request.FinYear
                  && d.BillType == "SI"
                  && d.SalesEntryType == "BG"
                  && d.SalesEntryId == request.SalesEntryId
            orderby d.FgId
            select new SalesInvoiceLineDto(
                d.FgId,
                brand.BrandTitle,
                fg.FgName,
                d.NoOfPacks,
                d.Weight,
                uom.UomTitle,
                d.BasicRate,
                d.SpecialDiscount,
                d.ExcStRate,
                d.IncStValue,
                d.StRate,
                d.StAmount,
                d.ExcStValue,
                d.NetRate,
                d.EmptyCartons,
                fg.NoOfUnitsInPack,
                fg.UnitWeight,
                fg.ForTonsConversion,
                fg.EmptyCartonPercentage,
                d.FreightCharges,
                d.SoapCharges,
                d.DistributorMargin,
                d.RetailerMargin,
                d.LeakageRate,
                d.VatPercentage,
                d.VatValue,
                d.Discount1Percentage,
                d.Discount1PercentageValue,
                d.Discount2PerPack,
                d.Discount2PerPackValue,
                fg.SgId,
                d.StAmount17,
                d.StAmount03)
        ).ToListAsync(cancellationToken);

        return new SalesInvoiceDto(
            header.FinYear,
            header.BillType,
            header.SalesEntryType,
            header.SalesEntryId,
            header.SalesEntryDate,
            header.VehicleRegNo,
            header.CustomerId,
            customerName,
            address,
            distShop,
            header.CustomerId == "99001",
            header.DeliveredTo,
            header.StockLocationId,
            header.TotalExcStValue,
            header.TotalVatValue,
            header.NetAmount,
            header.TotalWeightInTons,
            header.Checked,
            header.Authorized,
            lines);
    }
}

public record SearchCustomersQuery(byte CompanyId, string? Search, int Take = 30)
    : IRequest<IReadOnlyList<CustomerLookupDto>>;

public class SearchCustomersQueryHandler
    : IRequestHandler<SearchCustomersQuery, IReadOnlyList<CustomerLookupDto>>
{
    private readonly IApplicationDbContext _context;

    public SearchCustomersQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<CustomerLookupDto>> Handle(
        SearchCustomersQuery request,
        CancellationToken cancellationToken)
    {
        var take = request.Take is < 1 or > 100 ? 30 : request.Take;
        var query = _context.SalCustomerInfos.AsNoTracking()
            .Where(c => c.CompanyId == request.CompanyId && !c.Inactive);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLowerInvariant();
            query = query.Where(c =>
                c.CustomerId.ToLower().Contains(term)
                || c.CustomerCoName.ToLower().Contains(term));
        }

        return await query
            .OrderBy(c => c.CustomerCoName)
            .Take(take)
            .Select(c => new CustomerLookupDto(
                c.CustomerId,
                c.CustomerCoName,
                c.Address,
                c.IsDistShop))
            .ToListAsync(cancellationToken);
    }
}

public record SearchFinishedGoodsQuery(byte CompanyId, string? Search, int Take = 30)
    : IRequest<IReadOnlyList<FinishedGoodLookupDto>>;

public class SearchFinishedGoodsQueryHandler
    : IRequestHandler<SearchFinishedGoodsQuery, IReadOnlyList<FinishedGoodLookupDto>>
{
    private readonly IApplicationDbContext _context;

    public SearchFinishedGoodsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<FinishedGoodLookupDto>> Handle(
        SearchFinishedGoodsQuery request,
        CancellationToken cancellationToken)
    {
        var take = request.Take is < 1 or > 100 ? 30 : request.Take;

        var query =
            from fg in _context.InvItemFgs.AsNoTracking()
            join brand in _context.InvItemFgBrands.AsNoTracking() on fg.BrandId equals brand.BrandId
            join uom in _context.InvItemFgUoms.AsNoTracking() on fg.UomId equals uom.UomId
            where fg.CompanyId == request.CompanyId && !fg.Inactive
            select new { fg, brand, uom };

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.fg.FgId.ToString().Contains(term)
                || x.fg.FgName.ToLower().Contains(term)
                || x.brand.BrandTitle.ToLower().Contains(term));
        }

        return await query
            .OrderBy(x => x.fg.FgId)
            .Take(take)
            .Select(x => new FinishedGoodLookupDto(
                x.fg.FgId,
                x.brand.BrandTitle,
                x.fg.FgName,
                x.uom.UomTitle,
                x.fg.NoOfUnitsInPack,
                x.fg.UnitWeight,
                x.fg.ForTonsConversion,
                x.fg.StPercentage,
                x.fg.EmptyCartonPercentage,
                x.fg.DistributorMargin,
                x.fg.RetailerMargin,
                x.fg.SedPercentage,
                x.fg.SgId,
                x.fg.StPercentage,
                0,
                0,
                0,
                0,
                0))
            .ToListAsync(cancellationToken);
    }
}
