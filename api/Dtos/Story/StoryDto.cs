using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Story
{
    public class StoryDto
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; } = String.Empty;
        public DateTimeOffset CreatedDate { get; init; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedDate { get; set; } = DateTimeOffset.UtcNow;
        public int MaximumAuthors { get; set; } = 6;
        public int TurnDurationSeconds { get; set; } = 300;
    }
}