using Asp.Versioning;
using BusinessSuite.API.Extensions;
using BusinessSuite.Application.Features.SalCustomers.Commands;
using BusinessSuite.Application.Features.SalCustomers.Dtos;
using BusinessSuite.Application.Features.SalCustomers.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusinessSuite.API.Controllers.v1;

/// <summary>Customer Information against legacy tblSalCustomerInfo (VB parity).</summary>
[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/sal-customers")]
public class SalCustomersController : ControllerBase
{
    private readonly ISender _sender;

    public SalCustomersController(ISender sender) => _sender = sender;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 500,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        if (!User.TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        // Clamp so FluentValidation never trips on FE paging defaults.
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize switch
        {
            < 1 => 500,
            > 1000 => 1000,
            _ => pageSize
        };

        var result = await _sender.Send(
            new SearchSalCustomersQuery(companyId, pageNumber, pageSize, search),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("{customerId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string customerId, CancellationToken cancellationToken)
    {
        if (!User.TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var result = await _sender.Send(new GetSalCustomerByIdQuery(companyId, customerId), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] SaveSalCustomerRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var result = await _sender.Send(new CreateSalCustomerCommand(companyId, request), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { customerId = result.CustomerId, version = "1.0" }, result);
    }

    [HttpPut("{customerId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        string customerId,
        [FromBody] SaveSalCustomerRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new UpdateSalCustomerCommand(companyId, customerId, request),
            cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{customerId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string customerId, CancellationToken cancellationToken)
    {
        if (!User.TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        await _sender.Send(new DeleteSalCustomerCommand(companyId, customerId), cancellationToken);
        return NoContent();
    }
}
