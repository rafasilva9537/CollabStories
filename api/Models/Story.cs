using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models;

public class Story
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string Description { get; set; } = String.Empty;
    public DateTimeOffset CreatedDate { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedDate { get; set; } = DateTimeOffset.UtcNow;
    public int MaximumAuthors { get; set; } = 6;
    public int TurnDurationSeconds { get; set; } = 300;

    public ICollection<StoryPart> StoryParts { get; }= new List<StoryPart>();

    public string? OwnerUserId { get; set; }
    public AppUser OwnerUser { get; set; } = null!;
}
