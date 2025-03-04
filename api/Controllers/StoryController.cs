using System.Security.Claims;
using api.Constants;
using api.Dtos.Story;
using api.Dtos.StoryPart;
using api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[Authorize(Roles = RoleConstants.User)]
[ApiController]
[Route("collab-stories/")]
public class StoryController : ControllerBase
{
    private readonly IStoryService _storyService;
    public StoryController(IStoryService storyService)
    {
        _storyService = storyService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAllStories()
    {
        IList<StoryMainInfoDto> stories = await _storyService.GetStoriesAsync();

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
        StoryDto? story = await _storyService.GetStoryAsync(id);

        if(story is null) return NotFound();
        
        return Ok(story);
    }

    [HttpPost]
    public async Task<IActionResult> CreateStory([FromBody] CreateStoryDto createStory)
    {
        string? username = User.FindFirstValue(ClaimTypes.Name);
        if(username is null)
        {
            return BadRequest("Unable to find logged user");
        } 

        StoryDto newStory = await _storyService.CreateStoryAsync(createStory, username);

        return CreatedAtAction(nameof(this.GetStory), new { id = newStory.Id }, newStory);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateStory(int id, [FromBody] UpdateStoryDto updateStory)
    {
        string? loggedUser = User.FindFirstValue(ClaimTypes.Name);
        if(loggedUser is null)
        {
            return BadRequest("Unable to find logged user");
        } 

        StoryDto? updatedStory = await _storyService.UpdateStoryAsync(id, updateStory, loggedUser);

        if(updatedStory == null)
        {
            return NotFound();
        }

        return CreatedAtAction(nameof(this.GetStory), new { id = updatedStory.Id }, updatedStory);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteStory([FromRoute] int id)
    {
        bool isDeleted = await _storyService.DeleteStoryAsync(id);

        if(!isDeleted) return NotFound(new { Message = "Impossible to delete. Story doesn't exist." });

        return Ok( new {Message = "Story was successfully deleted!"} );
    }

    [AllowAnonymous]
    [HttpGet("{storyId:int}/story-parts")]
    public async Task<IActionResult> GetCompleteStory([FromRoute] int storyId)
    {
        CompleteStoryDto? completeStory = await _storyService.GetCompleteStoryAsync(storyId);

        if(completeStory == null) return NotFound();

        return Ok(completeStory);
    }

    [HttpPost("{storyId:int}/story-parts")]
    public async Task<IActionResult> CreateStoryPart([FromRoute] int storyId, [FromBody] CreateStoryPartDto storyPartDto)
    {
        StoryPartDto newStoryPart = await _storyService.CreateStoryPartAsync(storyId, storyPartDto);

        return Ok(newStoryPart);
    }
    
    [HttpDelete("{storyId:int}/story-parts/{storyPartId:int}")]
    public async Task<IActionResult> DeleteStoryPart([FromRoute] int storyId, [FromRoute] int storyPartId)
    {
        bool isDeleted = await _storyService.DeleteStoryPart(storyId, storyPartId);

        if(!isDeleted) return NotFound(new { Message = "Impossible to delete. Story part doesn't exist in specified Story." });

        return Ok( new {Message = "Story part was successfully deleted!"} );
    }
}