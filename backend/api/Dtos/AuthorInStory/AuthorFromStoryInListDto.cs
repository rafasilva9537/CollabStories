
namespace api.Dtos.AuthorInStory;

public record AuthorFromStoryInListDto
{
    public required string AuthorUserName { get; init; }
    public DateTimeOffset EntryDate { get; init; }
}
