using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Dtos.Story;
using api.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
            IList<StoryMainInfoDto> stories = await _repository.GetStoriesAsync();

            if(stories.Count == 0)
            {
                return NotFound();
            }

            return Ok(stories);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetStory(int id)
        {
            StoryDto? story = await _repository.GetStoryAsync(id);

            if(story == null)
            {
                return NotFound();
            }

            return Ok(story);
        }

        [HttpPost]
        public async Task<IActionResult> CreateStory([FromBody] CreateStoryDto createStory)
        {
            StoryDto newStory = await _repository.CreateStoryAsync(createStory);

            return CreatedAtAction(nameof(this.GetStory), new { id = newStory.Id }, newStory);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteStory([FromRoute] int id)
        {
            bool isDeleted = await _repository.DeleteStoryAsync(id);

            if(!isDeleted) return NotFound(new { Message = "Impossible to delete. Story doesn't exist" });

            return Ok( new {Message = "Story was successfully deleted!"} );
        }
    }
}