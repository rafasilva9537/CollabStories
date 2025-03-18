
namespace api.Dtos.AuthorInStory;

public class AuthorInStoryDto
{
    public required string AuthorUserName { get; set; }
    public int StoryId { get; set; }
    public DateTimeOffset EntryDate { get; set; }
}
