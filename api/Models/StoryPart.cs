using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class StoryPart
    {
        public int Id { get; set; }
        [Required]
        public required string Text { get; set; }

        public int StoryId { get; set; }
        public Story Story { get; set; } = null!;
        
    }
}