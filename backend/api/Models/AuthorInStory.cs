namespace api.Models;

public class AuthorInStory
{
    public int AuthorId { get; set; }
    public int StoryId { get; set; }
    public DateTimeOffset EntryDate { get; set; }

    public AppUser Author { get; set; } = null!;
    public Story Story { get; set; } = null!;
}