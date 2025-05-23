using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Story;

public class UpdateStoryDto
{
    [Required]
    [MaxLength(90)]
    public required string Title { get; set; }
    [MaxLength(200)]
    public string? Description { get; set; }
    [Range(1,16, ErrorMessage = "Authors quantity should be at least 1 and less or equal 16")]
    public int MaximumAuthors { get; set; }
    [Range(30,3600, ErrorMessage = "Turn duration should be at least 30 seconds and at maximum 1 hour")]
    public int TurnDurationSeconds { get; set; }
}