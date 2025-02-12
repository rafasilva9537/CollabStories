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
        public required string Text { get; set; }
        public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedDate { get; set; } = DateTimeOffset.UtcNow;

        public int StoryId { get; set; }
        public Story Story { get; set; } = null!;
        
    }
}