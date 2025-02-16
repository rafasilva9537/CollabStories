using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class Story
    {
        public int Id { get; set; }
        [MaxLength(90)]
        public required string Title { get; set; }
        [MaxLength(200)]
        public string Description { get; set; } = String.Empty;
        public DateTimeOffset CreatedDate { get; init; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedDate { get; set; } = DateTimeOffset.UtcNow;
        [Range(1,16, ErrorMessage = "Authors quantity should be at least 1 and less or equal 16")]
        public int MaximumAuthors { get; set; } = 6;
        [Range(30,3600, ErrorMessage = "Turn duration should be at least 30 seconds and at maximum 1 hour")]
        public int TurnDurationSeconds { get; set; } = 300;

        public ICollection<StoryPart> StoryParts { get; }= new List<StoryPart>();
    }
}