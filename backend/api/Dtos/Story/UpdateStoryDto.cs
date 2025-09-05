using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Story;

public record UpdateStoryDto
{
    [MinLength(1, ErrorMessage = "Title should be at least 1 character long")]
    [MaxLength(90, ErrorMessage = "Title should be at maximum 90 characters long")]
    public string? Title { get; init; }
    
    [MinLength(1, ErrorMessage = "Description should be at least 1 character long")]
    [MaxLength(200, ErrorMessage = "Description should be at maximum 200 characters long")]
    public string? Description { get; init; }
    
    [Range(1,16, ErrorMessage = "Authors quantity should be at least 1 and less or equal to 16")]
    public int? MaximumAuthors { get; init; }
    
    [Range(30,3600, ErrorMessage = "Turn duration should be at least 30 seconds and at maximum 1 hour")]
    public int? TurnDurationSeconds { get; init; }
}