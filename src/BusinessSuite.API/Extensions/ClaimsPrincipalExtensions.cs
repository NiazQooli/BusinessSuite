using System.Security.Claims;

namespace BusinessSuite.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Login subject is "{CompanyId}:{UserId}".
    /// </summary>
    public static bool TryGetCompanyAndUser(this ClaimsPrincipal user, out byte companyId, out string userId)
    {
        companyId = 0;
        userId = string.Empty;

        var subject = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub");

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

        var companyClaim = user.FindFirstValue("company_id");
        userId = user.FindFirstValue("unique_name")
            ?? user.FindFirstValue(ClaimTypes.Name)
            ?? string.Empty;

        return byte.TryParse(companyClaim, out companyId) && !string.IsNullOrWhiteSpace(userId);
    }

    public static bool TryGetCompanyId(this ClaimsPrincipal user, out byte companyId)
        => user.TryGetCompanyAndUser(out companyId, out _);
}
