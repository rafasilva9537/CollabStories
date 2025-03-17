namespace api.Models;

public class Story
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset CreatedDate { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedDate { get; set; } = DateTimeOffset.UtcNow;
    public int MaximumAuthors { get; set; } = 6;
    public int TurnDurationSeconds { get; set; } = 300;

    public ICollection<StoryPart> StoryParts { get; }= [];
    public AppUser User { get; set; } = null!;
    public ICollection<AuthorInStory> AuthorInStory { get; set; } = [];
}
