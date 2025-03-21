using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace api.Models;

// TODO: change UserId to INT type or add a new unique autoincrement column to be used internally
public class AppUser : IdentityUser<int>
{
    public override string UserName { get; set; } = string.Empty;
    public override string Email { get; set; } = string.Empty;

    public string Nickname { get; set; } = string.Empty;
    public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.UtcNow;
    public string Description { get; set; } = string.Empty;
    public string ProfileImage { get; set; } = string.Empty;

    public ICollection<Story> Stories { get; set; } = [];
    public ICollection<StoryPart> StoryParts { get; set; } = [];
    public ICollection<AuthorInStory> AuthorInStory { get; set; } = [];
}