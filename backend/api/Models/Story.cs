namespace api.Models;

public class Story
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    // TODO: make time exactly the same for the three bellow at story creation
    public DateTimeOffset CreatedDate { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedDate { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset AuthorsMembershipChangeDate { get; set; } = DateTimeOffset.UtcNow;
    public int MaximumAuthors { get; set; } = 6;
    public int TurnDurationSeconds { get; set; } = 300;
    public bool IsFinished { get; set; }
    public int? CurrentAuthorId { get; set; }

    public ICollection<StoryPart> StoryParts { get; } = [];
    public AppUser? User { get; set; }
    public AppUser? CurrentAuthor { get; set; }
    public IList<AuthorInStory> AuthorsInStory { get; } = [];
}
