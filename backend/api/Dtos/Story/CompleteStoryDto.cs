using api.Dtos.AuthorInStory;
using api.Dtos.StoryPart;

namespace api.Dtos.Story;

// TODO: replace set with init. Solve all the conflicts generated because of the change
public record CompleteStoryDto
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset UpdatedDate { get; set; }
    public int MaximumAuthors { get; set; }
    public int TurnDurationSeconds { get; set; }
    public ICollection<AuthorFromStoryInListDto> StoryAuthors { get; set; } = [];
}