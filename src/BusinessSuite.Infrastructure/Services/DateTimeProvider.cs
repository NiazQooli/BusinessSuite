using BusinessSuite.Application.Common.Interfaces;

namespace BusinessSuite.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
