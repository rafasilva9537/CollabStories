
namespace api.Dtos.AuthorInStory;

public record AuthorInStoryDto
{
    public required string AuthorUserName { get; init; }
    public int StoryId { get; init; }
    public DateTimeOffset EntryDate { get; init; }
}
