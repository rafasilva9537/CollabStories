namespace api.Dtos.Story;

public record StoryInfoForSessionDto
{
    public DateTimeOffset AuthorsMembershipChangeDate { get; init; }
    public int TurnDurationSeconds { get; init; }
}