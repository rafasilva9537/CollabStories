using System.Security.Claims;
using api.Constants;
using api.Dtos.Story;
using api.Dtos.StoryPart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using api.Dtos.HttpResponses;
using api.Dtos.Pagination;
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
        PagedKeysetStoryList<StoryMainInfoDto> stories = await _storyService.GetStoriesAsync(lastId);

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
        
        if(loggedUser is null) return Forbid();
        if(!await _storyService.IsStoryOwner(loggedUser, storyId)) return Forbid();

        if (string.IsNullOrEmpty(updateStory.Title) &&
            updateStory.Description is null && 
            updateStory.MaximumAuthors is null &&
            updateStory.TurnDurationSeconds is null)
        {
            ModelState.AddModelError("AllFieldsEmpty", "At least one field must be updated.");
            return ValidationProblem(ModelState);
        };
        
        StoryDto updatedStory = await _storyService.UpdateStoryAsync(storyId, updateStory);
        return CreatedAtAction(nameof(GetStory), new { id = updatedStory.Id }, updatedStory);
    }

    [HttpDelete("{storyId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MessageResponse>> DeleteStory([FromRoute] int storyId)
    {
        string? loggedUser = User.FindFirstValue(ClaimTypes.Name);
        if(loggedUser is null)
        {
            return Forbid();
        }

        if(!await _storyService.IsStoryOwner(loggedUser, storyId))
        {
            return Forbid();
        }

        bool isDeleted = await _storyService.DeleteStoryAsync(storyId);
        if(!isDeleted) return NotFound();

        return Ok();
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

        if(completeStory is null) return NotFound();

        return Ok(completeStory);
    }

    [Authorize(Policy = PolicyConstants.RequiredAdminRole)]
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
        if (loggedUser is null) return Unauthorized();

        if (!await _storyService.IsStoryPartCreator(loggedUser, storyPartId)) return Forbid();

        bool isDeleted = await _storyService.DeleteStoryPart(storyId, storyPartId);
        if (!isDeleted) return NotFound();

        return Ok();
    }
}