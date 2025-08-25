using api.Interfaces;

namespace api.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow { get; } = DateTimeOffset.UtcNow;
    public DateTime UtcNowDateTime { get; } = DateTime.UtcNow;
}