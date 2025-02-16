using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.StoryPart
{
    public class StoryPartDto
    {
        public int Id { get; set; }
        public required string Text { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public int StoryId { get; set; }
    }
}