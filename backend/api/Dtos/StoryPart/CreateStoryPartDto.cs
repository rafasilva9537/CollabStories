using System.ComponentModel.DataAnnotations;

namespace api.Dtos.StoryPart
{
    public record CreateStoryPartDto
    {
        [Required]
        public required string Text { get; init; }
    }
}