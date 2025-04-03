using api.Dtos.AuthorInStory;
using api.Dtos.StoryPart;

namespace api.Dtos.Story;

public class CompleteStoryDto
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset CreatedDate { get; init; }
    public DateTimeOffset UpdatedDate { get; set; }
    public int MaximumAuthors { get; set; }
    public int TurnDurationSeconds { get; set; }
    public ICollection<StoryPartInListDto> StoryParts { get; set; } = [];
    public ICollection<AuthorFromStoryInListDto> StoryAuthors { get; set; } = [];
}