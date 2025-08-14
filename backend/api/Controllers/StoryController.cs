using System.Security.Claims;
using api.Constants;
using api.Dtos.Story;
using api.Dtos.StoryPart;
using api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using api.Dtos.HttpResponses;
using api.Interfaces;

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
    public async Task<ActionResult<IList<StoryMainInfoDto>>> GetStories([FromQuery] int? lastId)
    {
        IList<StoryMainInfoDto> stories = await _storyService.GetStoriesAsync(lastId);

        return Ok(stories);
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<ActionResult<StoryDto>> GetStory(int id)
    {
        StoryDto? story = await _storyService.GetStoryAsync(id);

        if(story is null) return NotFound(new MessageResponse { Message = "No story exists." });
        
        return Ok(story);
    }

    [HttpPost]
    public async Task<ActionResult<StoryDto>> CreateStory([FromBody] CreateStoryDto createStory)
    {
        string? username = User.FindFirstValue(ClaimTypes.Name);
        if(username is null)
        {
            return BadRequest("Unable to find logged user");
        } 

        StoryDto newStory = await _storyService.CreateStoryAsync(createStory, username);

        return CreatedAtAction(nameof(this.GetStory), new { id = newStory.Id }, newStory);
    }

    [HttpPut("{storyId}")]
    public async Task<ActionResult<StoryDto>> UpdateStory(int storyId, [FromBody] UpdateStoryDto updateStory)
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

    [HttpDelete("{storyId}")]
    public async Task<ActionResult<MessageResponse>> DeleteStory([FromRoute] int storyId)
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

        if(!isDeleted) return NotFound(new MessageResponse { Message = "Impossible to delete. Story doesn't exist." });

        return Ok(new MessageResponse { Message = "Story was successfully deleted!" } );
    }


    [HttpPost("{storyId}/join")]
    public async Task<ActionResult<MessageResponse>> JoinStory([FromRoute] int storyId)
    {
        string? loggedUser = User.FindFirstValue(ClaimTypes.Name);
        if(loggedUser is null)
        {
            return Forbid();
        }

        bool joinedStory = await _storyService.JoinStoryAsync(loggedUser, storyId);

        if(joinedStory == false) return Forbid();
        
        return Ok(new MessageResponse { Message = "User joined story." });
    }

    [HttpPost("{storyId}/leave")]
    public async Task<ActionResult<MessageResponse>> LeaveStory([FromRoute] int storyId)
    {
        string? loggedUser = User.FindFirstValue(ClaimTypes.Name);
        if(loggedUser is null)
        {
            return Forbid();
        }

        bool leftStory = await _storyService.LeaveStoryAsync(loggedUser, storyId);
        if(leftStory == false) return Forbid();

        return Ok(new MessageResponse { Message = "User left story." });
    }


    [AllowAnonymous]
    [HttpGet("{storyId}/story-parts")]
    public async Task<ActionResult<CompleteStoryDto>> GetCompleteStory([FromRoute] int storyId)
    {
        CompleteStoryDto? completeStory = await _storyService.GetCompleteStoryAsync(storyId);

        if(completeStory == null) return NotFound();

        return Ok(completeStory);
    }

    [HttpPost("{storyId}/story-parts")]
    public async Task<ActionResult<StoryPartDto>> CreateStoryPart([FromRoute] int storyId, [FromBody] CreateStoryPartDto storyPartDto)
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
    
    [HttpDelete("{storyId}/story-parts/{storyPartId}")]
    public async Task<ActionResult<MessageResponse>> DeleteStoryPart([FromRoute] int storyId, [FromRoute] int storyPartId)
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

        if(!isDeleted) return NotFound(new MessageResponse { Message = "Impossible to delete. Story part doesn't exist in specified Story." });

        return Ok(new MessageResponse { Message = "Story part was successfully deleted!" } );
    }
}