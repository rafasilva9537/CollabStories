namespace api.Interfaces;

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
    DateTime UtcNowDateTime { get; }
}