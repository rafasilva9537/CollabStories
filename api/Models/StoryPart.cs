namespace api.Models;

public class StoryPart
{
    public int Id { get; set; }
    public required string Text { get; set; }
    public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.UtcNow;

    public int StoryId { get; set; }
    public Story Story { get; set; } = null!;
    
    public string? OwnerUserId { get; set; }
    public AppUser OwnerUser { get; set; } = null!;
}