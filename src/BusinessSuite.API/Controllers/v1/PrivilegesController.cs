using System.Security.Claims;
using Asp.Versioning;
using BusinessSuite.Application.Features.Privileges.Queries.GetUserPrivileges;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusinessSuite.API.Controllers.v1;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class PrivilegesController : ControllerBase
{
    private readonly ISender _sender;

    public PrivilegesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Returns menu privileges for the signed-in user.
    /// Source: legacy tblAdmUserPrivilege (CompanyID + UserID).
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        if (!TryGetIdentity(out var companyId, out var userId))
        {
            return Unauthorized(new { error = "Token is missing company or user identity." });
        }

        var result = await _sender.Send(new GetUserPrivilegesQuery(companyId, userId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Subject claim is "{CompanyId}:{UserId}" from login. company_id is a backup claim.
    /// </summary>
    private bool TryGetIdentity(out byte companyId, out string userId)
    {
        companyId = 0;
        userId = string.Empty;

        var subject = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (!string.IsNullOrWhiteSpace(subject))
        {
            var separator = subject.IndexOf(':');
            if (separator > 0
                && byte.TryParse(subject[..separator], out companyId)
                && separator < subject.Length - 1)
            {
                userId = subject[(separator + 1)..];
                return true;
            }
        }

        var companyClaim = User.FindFirstValue("company_id");
        userId = User.FindFirstValue("unique_name")
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? string.Empty;

        return byte.TryParse(companyClaim, out companyId) && !string.IsNullOrWhiteSpace(userId);
    }
}
