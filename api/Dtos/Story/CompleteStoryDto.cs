using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.StoryPart;

namespace api.Dtos.Story
{
    public class CompleteStoryDto
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public DateTimeOffset CreatedDate { get; init; }
        public DateTimeOffset UpdatedDate { get; set; }
        public int MaximumAuthors { get; set; }
        public int TurnDurationSeconds { get; set; }
        public ICollection<StoryPartDto> StoryParts { get; set; } = new List<StoryPartDto>();
    }
}