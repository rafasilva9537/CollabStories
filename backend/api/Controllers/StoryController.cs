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
    public async Task<IActionResult> GetStories([FromRoute] int lastId)
    {
        IList<StoryMainInfoDto> stories = await _storyService.GetStoriesAsync(lastId);

        if(stories.Count == 0)
        {
            return Ok(new { message = "No story exists." });
        }

        return Ok(stories);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetStory(int id)
    {
        StoryDto? story = await _storyService.GetStoryAsync(id);

        if(story is null) return NotFound(new { message = "No story exists." });
        
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

    [HttpPut("{storyId:int}")]
    public async Task<IActionResult> UpdateStory(int storyId, [FromBody] UpdateStoryDto updateStory)
    {
        string? loggedUser = User.FindFirstValue(ClaimTypes.Name);
        if(loggedUser is null)
        {
            return Forbid();
        }

        if(!await _storyService.IsStoryCreator(loggedUser, storyId))
        {
            return Forbid();
        }

        StoryDto? updatedStory = await _storyService.UpdateStoryAsync(storyId, updateStory);

        if(updatedStory == null)
        {
            return NotFound();
        }

        return CreatedAtAction(nameof(this.GetStory), new { id = updatedStory.Id }, updatedStory);
    }

    [HttpDelete("{storyId:int}")]
    public async Task<IActionResult> DeleteStory([FromRoute] int storyId)
    {
        string? loggedUser = User.FindFirstValue(ClaimTypes.Name);
        if(loggedUser is null)
        {
            return Forbid();
        }

        if(!await _storyService.IsStoryCreator(loggedUser, storyId))
        {
            return Forbid();
        }

        bool isDeleted = await _storyService.DeleteStoryAsync(storyId);

        if(!isDeleted) return NotFound(new { message = "Impossible to delete. Story doesn't exist." });

        return Ok( new { message = "Story was successfully deleted!" } );
    }


    [HttpPost("{storyId:int}/join")]
    public async Task<IActionResult> JoinStory([FromRoute] int storyId)
    {
        string? loggedUser = User.FindFirstValue(ClaimTypes.Name);
        if(loggedUser is null)
        {
            return Forbid();
        }

        bool joinedStory = await _storyService.JoinStoryAsync(loggedUser, storyId);

        if(joinedStory == false) return Forbid();
        
        return Ok(new { message = "User joined story." });
    }

    [HttpPost("{storyId:int}/leave")]
    public async Task<IActionResult> LeaveStory([FromRoute] int storyId)
    {
        string? loggedUser = User.FindFirstValue(ClaimTypes.Name);
        if(loggedUser is null)
        {
            return Forbid();
        }

        bool leftStory = await _storyService.LeaveStoryAsync(loggedUser, storyId);
        if(leftStory == false) return Forbid();

        return Ok(new { message = "User left story." });
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
        string? loggedUser = User.FindFirstValue(ClaimTypes.Name);
        if(loggedUser is null)
        {
            return Forbid();
        }

        StoryPartDto? newStoryPart = await _storyService.CreateStoryPartAsync(storyId, loggedUser, storyPartDto);

        if(newStoryPart is null) return Forbid();

        return Ok(newStoryPart);
    }
    
    [HttpDelete("{storyId:int}/story-parts/{storyPartId:int}")]
    public async Task<IActionResult> DeleteStoryPart([FromRoute] int storyId, [FromRoute] int storyPartId)
    {
        string? loggedUser = User.FindFirstValue(ClaimTypes.Name);
        if(loggedUser is null)
        {
            return Forbid();
        }

        if(!await _storyService.IsStoryPartCreator(loggedUser, storyPartId))
        {
            return Forbid();
        }

        bool isDeleted = await _storyService.DeleteStoryPart(storyId, storyPartId);

        if(!isDeleted) return NotFound(new { message = "Impossible to delete. Story part doesn't exist in specified Story." });

        return Ok( new { message = "Story part was successfully deleted!" } );
    }
}