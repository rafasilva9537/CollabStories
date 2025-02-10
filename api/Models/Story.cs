using System;
using System.Collections.Generic;
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
        public ICollection<StoryPart> StoryParts = new List<StoryPart>();
    }
}