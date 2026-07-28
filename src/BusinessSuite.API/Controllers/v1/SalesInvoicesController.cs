using BusinessSuite.API.Extensions;
using BusinessSuite.Application.Features.SalesInvoices.Commands;
using BusinessSuite.Application.Features.SalesInvoices.Dtos;
using BusinessSuite.Application.Features.SalesInvoices.Queries;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusinessSuite.API.Controllers.v1;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/sales-invoices")]
public class SalesInvoicesController : ControllerBase
{
    private readonly ISender _sender;

    public SalesInvoicesController(ISender sender) => _sender = sender;

    [HttpGet("context")]
    public async Task<IActionResult> GetContext(CancellationToken cancellationToken)
    {
        if (!User.TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        return Ok(await _sender.Send(new GetSalesInvoiceContextQuery(companyId), cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string finYear,
        [FromQuery] string? search = null,
        [FromQuery] string? searchField = null,
        [FromQuery] string? sortOn = null,
        [FromQuery] bool? sortFlag = null,
        [FromQuery] DateTime? periodStart = null,
        [FromQuery] DateTime? periodEnd = null,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        if (!User.TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(finYear))
        {
            return BadRequest(new { error = "finYear is required." });
        }

        var result = await _sender.Send(
            new SearchSalesInvoicesQuery(
                companyId,
                finYear.Trim(),
                search,
                searchField,
                sortOn,
                sortFlag,
                periodStart,
                periodEnd,
                take),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("{salesEntryId:int}")]
    public async Task<IActionResult> GetById(
        int salesEntryId,
        [FromQuery] string finYear,
        CancellationToken cancellationToken = default)
    {
        if (!User.TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new GetSalesInvoiceQuery(companyId, finYear.Trim(), salesEntryId),
            cancellationToken);

        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] SaveSalesInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetCompanyAndUser(out var companyId, out var userId))
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new SaveSalesInvoiceCommand(companyId, userId, null, request),
            cancellationToken);
        return Ok(result);
    }

    [HttpPut("{salesEntryId:int}")]
    public async Task<IActionResult> Update(
        int salesEntryId,
        [FromBody] SaveSalesInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetCompanyAndUser(out var companyId, out var userId))
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new SaveSalesInvoiceCommand(companyId, userId, salesEntryId, request),
            cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{salesEntryId:int}")]
    public async Task<IActionResult> Delete(
        int salesEntryId,
        [FromQuery] string finYear,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetCompanyAndUser(out var companyId, out var userId))
        {
            return Unauthorized();
        }

        await _sender.Send(
            new DeleteSalesInvoiceCommand(companyId, finYear.Trim(), salesEntryId, userId),
            cancellationToken);
        return NoContent();
    }

    [HttpPost("{salesEntryId:int}/check")]
    public async Task<IActionResult> Check(
        int salesEntryId,
        [FromQuery] string finYear,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetCompanyAndUser(out var companyId, out var userId))
        {
            return Unauthorized();
        }

        await _sender.Send(
            new CheckSalesInvoiceCommand(companyId, finYear.Trim(), salesEntryId, userId),
            cancellationToken);
        return NoContent();
    }

    [HttpPost("{salesEntryId:int}/post")]
    public async Task<IActionResult> Post(
        int salesEntryId,
        [FromQuery] string finYear,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetCompanyAndUser(out var companyId, out var userId))
        {
            return Unauthorized();
        }

        await _sender.Send(
            new PostSalesInvoiceCommand(companyId, finYear.Trim(), salesEntryId, userId),
            cancellationToken);
        return NoContent();
    }

    [HttpGet("lookups/customers")]
    public async Task<IActionResult> SearchCustomers(
        [FromQuery] string? search = null,
        [FromQuery] int take = 30,
        CancellationToken cancellationToken = default)
    {
        if (!User.TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        return Ok(await _sender.Send(new SearchCustomersQuery(companyId, search, take), cancellationToken));
    }

    [HttpGet("lookups/finished-goods")]
    public async Task<IActionResult> SearchFinishedGoods(
        [FromQuery] string? search = null,
        [FromQuery] int take = 30,
        CancellationToken cancellationToken = default)
    {
        if (!User.TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        return Ok(await _sender.Send(new SearchFinishedGoodsQuery(companyId, search, take), cancellationToken));
    }
}
