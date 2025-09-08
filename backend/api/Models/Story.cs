namespace api.Models;

public class Story
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset UpdatedDate { get; set; }
    public DateTimeOffset AuthorsMembershipChangeDate { get; set; }
    public int MaximumAuthors { get; set; } = 6;
    public int TurnDurationSeconds { get; set; } = 300;
    public bool IsFinished { get; set; }
    public int? CurrentAuthorId { get; set; }

    public ICollection<StoryPart> StoryParts { get; } = [];
    public AppUser? User { get; set; }
    public AppUser? CurrentAuthor { get; set; }
    public List<AuthorInStory> AuthorsInStory { get; } = [];
}
