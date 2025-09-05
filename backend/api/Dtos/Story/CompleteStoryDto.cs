using api.Dtos.AuthorInStory;
using api.Dtos.StoryPart;

namespace api.Dtos.Story;

public record CompleteStoryDto
{
    public required int Id { get; init; }
    public required string Title { get; init; }
    public required string? Description { get; init; }
    public required DateTimeOffset CreatedDate { get; init; }
    public required DateTimeOffset UpdatedDate { get; init; }
    public required int MaximumAuthors { get; init; }
    public required int TurnDurationSeconds { get; init; }
    public required string? StoryOwner { get; init; }
    public required string? CurrentAuthor { get; init; }
    public IList<AuthorFromStoryInListDto> StoryAuthors { get; init; } = [];
}