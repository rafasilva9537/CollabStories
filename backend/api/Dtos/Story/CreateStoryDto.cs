using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Story;

public record CreateStoryDto
{
    [Required]
    [MaxLength(90)]
    public required string Title { get; init; }
    [MaxLength(200)]
    public string? Description { get; init; }
    [Range(1,16, ErrorMessage = "Authors quantity should be at least 1 and less or equal 16")]
    public int MaximumAuthors { get; init; }
    [Range(30,3600, ErrorMessage = "Turn duration should be at least 30 seconds and at maximum 1 hour")]
    public int TurnDurationSeconds { get; init; }
}