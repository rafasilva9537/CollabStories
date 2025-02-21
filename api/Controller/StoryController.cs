using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Story;
using api.Dtos.StoryPart;
using api.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controller;

[Authorize]
[ApiController]
[Route("collab-stories/")]
public class StoryController : ControllerBase
{
    private readonly IStoryRepository _storyRepository;
    public StoryController(IStoryRepository repository)
    {
        _storyRepository = repository;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAllStories()
    {
        IList<StoryMainInfoDto> stories = await _storyRepository.GetStoriesAsync();

        if(stories.Count == 0)
        {
            return Ok(new { Message = "No story exists." });
        }

        return Ok(stories);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetStory(int id)
    {
        StoryDto? story = await _storyRepository.GetStoryAsync(id);

        if(story is null) return NotFound();
        
        return Ok(story);
    }

    [HttpPost]
    public async Task<IActionResult> CreateStory([FromBody] CreateStoryDto createStory)
    {
        StoryDto newStory = await _storyRepository.CreateStoryAsync(createStory);

        return CreatedAtAction(nameof(this.GetStory), new { id = newStory.Id }, newStory);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateStory(int id, [FromBody] UpdateStoryDto updateStory)
    {
        StoryDto? updatedStory = await _storyRepository.UpdateStoryAsync(id, updateStory);

        if(updatedStory == null)
        {
            return NotFound();
        }

        return CreatedAtAction(nameof(this.GetStory), new { id = updatedStory.Id }, updatedStory);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteStory([FromRoute] int id)
    {
        bool isDeleted = await _storyRepository.DeleteStoryAsync(id);

        if(!isDeleted) return NotFound(new { Message = "Impossible to delete. Story doesn't exist." });

        return Ok( new {Message = "Story was successfully deleted!"} );
    }

    [AllowAnonymous]
    [HttpGet("{storyId:int}/story-parts")]
    public async Task<IActionResult> GetCompleteStory([FromRoute] int storyId)
    {
        CompleteStoryDto? completeStory = await _storyRepository.GetCompleteStoryAsync(storyId);

        if(completeStory == null) return NotFound();

        return Ok(completeStory);
    }

    [HttpPost("{storyId:int}/story-parts")]
    public async Task<IActionResult> CreateStoryPart([FromRoute] int storyId, [FromBody] CreateStoryPartDto storyPartDto)
    {
        StoryPartDto newStoryPart = await _storyRepository.CreateStoryPartAsync(storyId, storyPartDto);

        return Ok(newStoryPart);
    }
    
    [HttpDelete("{storyId:int}/story-parts/{storyPartId:int}")]
    public async Task<IActionResult> DeleteStoryPart([FromRoute] int storyId, [FromRoute] int storyPartId)
    {
        bool isDeleted = await _storyRepository.DeleteStoryPart(storyId, storyPartId);

        if(!isDeleted) return NotFound(new { Message = "Impossible to delete. Story part doesn't exist in specified Story." });

        return Ok( new {Message = "Story part was successfully deleted!"} );
    }
}