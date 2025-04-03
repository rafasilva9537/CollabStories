
namespace api.Dtos.AuthorInStory;

public class AuthorFromStoryInListDto
{
    public required string AuthorUserName { get; set; }
    public DateTimeOffset EntryDate { get; set; }
}
