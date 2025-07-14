namespace api.Dtos.Story;

public record StoryInfoForSessionDto
{
    public DateTimeOffset UpdatedDate { get; init; }
    public int TurnDurationSeconds { get; init; }
}