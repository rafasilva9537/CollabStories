using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace api.Models;

public class AppUser : IdentityUser
{
    public override required string UserName { get; set; }
    public override required string Email { get; set; }

    public string Nickname { get; set; } = String.Empty;
    public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.UtcNow;
    public string Description { get; set; } = String.Empty;

    public ICollection<Story> Stories { get; set; } = new List<Story>();
    public ICollection<StoryPart> StoryParts { get; set; } = new List<StoryPart>();
}