using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controller
{
    [ApiController]
    [Route("collab-stories/")]
    public class StoryController : ControllerBase
    {
        private readonly IStoryRepository _repository;
        public StoryController(IStoryRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStories()
        {
            var stories = await _repository.GetStoriesAsync();

            if(stories.Count == 0)
            {
                return NotFound();
            }

            return Ok(stories);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetStory(int id)
        {
            var story = await _repository.GetStoryAsync(id);

            if(story == null)
            {
                return NotFound();
            }

            return Ok(story);
        }
    }
}