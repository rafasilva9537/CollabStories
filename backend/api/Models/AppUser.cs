using Microsoft.AspNetCore.Identity;

namespace api.Models;

public class AppUser : IdentityUser<int>
{
    public override string UserName { get; set; } = string.Empty;
    public override string Email { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
    public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.UtcNow;
    public string Description { get; set; } = string.Empty;
    public string ProfileImage { get; set; } = string.Empty;

    public ICollection<Story> Stories { get;} = [];
    public ICollection<Story> CurrentAuthorStories { get; } = [];
    public ICollection<StoryPart> StoryParts { get; } = [];
    public ICollection<AuthorInStory> AuthorInStory { get; } = [];
}