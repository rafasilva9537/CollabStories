namespace api.Interfaces;

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}