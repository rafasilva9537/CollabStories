using api.Interfaces;

namespace api.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow  => DateTimeOffset.UtcNow;
    public DateTime UtcNowDateTime  => DateTime.UtcNow;
}