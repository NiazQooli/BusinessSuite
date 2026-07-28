using System.Security.Claims;
using Asp.Versioning;
using BusinessSuite.Application.Features.Menus.Queries.GetMenuItems;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusinessSuite.API.Controllers.v1;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class MenusController : ControllerBase
{
    private readonly ISender _sender;

    public MenusController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Returns menu catalog for the signed-in user's company.
    /// Source: legacy tblSftMenuItems. Inactive items excluded by default.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetForCompany(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized(new { error = "Token is missing company identity." });
        }

        var result = await _sender.Send(
            new GetMenuItemsQuery(companyId, includeInactive),
            cancellationToken);

        return Ok(result);
    }

    private bool TryGetCompanyId(out byte companyId)
    {
        companyId = 0;

        var companyClaim = User.FindFirstValue("company_id");
        if (byte.TryParse(companyClaim, out companyId))
        {
            return true;
        }

        var subject = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(subject))
        {
            return false;
        }

        var separator = subject.IndexOf(':');
        return separator > 0 && byte.TryParse(subject[..separator], out companyId);
    }
}
