namespace api.Models;

public class StoryPart
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.UtcNow;
    public int StoryId { get; set; }
    public int? UserId { get; set; }

    public Story Story { get; set; } = null!;
    public AppUser User { get; set; } = null!;
}