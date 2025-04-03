using System.ComponentModel.DataAnnotations;

namespace api.Dtos.StoryPart
{
    public class CreateStoryPartDto
    {
        [Required]
        public required string Text { get; set; }
    }
}