using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using api.Models;
using System.ComponentModel.DataAnnotations;

namespace api.Dtos.StoryPart
{
    public class CreateStoryPartDto
    {
        [Required]
        public required string Text { get; set; }
    }
}