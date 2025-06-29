namespace api.Dtos.Story;

// TODO: replace UserName set with init. Solve all the conflicts generated because of the change
public record StoryDto
{
    public int Id { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public DateTimeOffset CreatedDate { get; init; }
    public DateTimeOffset UpdatedDate { get; init; }
    public int MaximumAuthors { get; init; }
    public int TurnDurationSeconds { get; init; }
    public string? UserName { get; set; }
}