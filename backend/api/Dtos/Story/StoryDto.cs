namespace api.Dtos.Story;

public class StoryDto
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset CreatedDate { get; init; }
    public DateTimeOffset UpdatedDate { get; set; }
    public int MaximumAuthors { get; set; }
    public int TurnDurationSeconds { get; set; }
    public string? UserName { get; set; }
}