using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controller
{
    [ApiController]
    [Route("collab-stories/[controller]")]
    public class StoryController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        public StoryController(ApplicationDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetStories()
        {
            var stories = await _context.Story.ToListAsync();
            
            return Ok(new { Message = "Hello World!" });
        }
    }
}