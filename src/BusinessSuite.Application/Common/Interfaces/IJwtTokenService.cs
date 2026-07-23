namespace BusinessSuite.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string CreateToken(string subject, string userName, string displayName, int? companyId = null);
    DateTime GetExpiryUtc();
}
