using System.Security.Claims;
using api.Constants;
using api.Dtos.Story;
using api.Dtos.StoryPart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using api.Dtos.HttpResponses;
using api.Dtos.Pagination;
using api.Exceptions;
using api.Interfaces;

namespace api.Controllers;

[Authorize(Roles = RoleConstants.User)]
[ApiController]
[Route("collab-stories/")]
public class StoryController : ControllerBase
{
    private readonly IStoryService _storyService;
    private readonly ILogger<StoryController> _logger;
    public StoryController(IStoryService storyService, ILogger<StoryController> logger)
    {
        _storyService = storyService;
        _logger = logger;
    }
    
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(PagedKeysetStoryList<StoryMainInfoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedKeysetStoryList<StoryMainInfoDto>>> GetStories([FromQuery] int? lastId)
    {
        PagedKeysetStoryList<StoryMainInfoDto> stories = await _storyService.GetStoriesAsync(lastId);

        return Ok(stories);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(StoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StoryDto>> GetStory(int id)
    {
        StoryDto? story = await _storyService.GetStoryAsync(id);

        if(story is null) return NotFound();
        
        return Ok(story);
    }

    [HttpPost]
    [ProducesResponseType(typeof(StoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<StoryDto>> CreateStory([FromBody] CreateStoryDto createStory)
    {
        string? userName = User.FindFirstValue(ClaimTypes.Name);
        try
        {
            if(userName is null) return Unauthorized();

            StoryDto newStory = await _storyService.CreateStoryAsync(createStory, userName);

            return CreatedAtAction(nameof(GetStory), new { id = newStory.Id }, newStory);
        }
        catch (UserNotFoundException ex)
        {
            _logger.LogError(ex,"Unexpected logged user '{UserName}' not found at story creation", userName);
            return Unauthorized();
        }
    }

    [HttpPut("{storyId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> UpdateStory(int storyId, [FromBody] UpdateStoryDto updateStory)
    {
        try
        {
            string? loggedUser = User.FindFirstValue(ClaimTypes.Name);
        
            if (loggedUser is null) return Unauthorized();
            if (!await _storyService.IsStoryOwner(loggedUser, storyId)) return Forbid();

            if (string.IsNullOrEmpty(updateStory.Title) &&
                updateStory.Description is null &&
                updateStory.MaximumAuthors is null &&
                updateStory.TurnDurationSeconds is null)
            {
                ModelState.AddModelError("AllFieldsEmpty", "At least one field must be updated.");
                return ValidationProblem(ModelState);
            };
        
            await _storyService.UpdateStoryAsync(storyId, updateStory);
            return Ok();
        }
        catch (StoryNotFoundException ex)
        {
            _logger.LogError(ex, "Unexpected story '{StoryId}' not found at story update", storyId);
            return NotFound();
        }
    }

    [HttpDelete("{storyId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MessageResponse>> DeleteStory([FromRoute] int storyId)
    {
        string? loggedUser = User.FindFirstValue(ClaimTypes.Name);
        if (loggedUser is null) return Unauthorized();

        if (!await _storyService.IsStoryOwner(loggedUser, storyId))
        {
            return Forbid();
        }

        bool isDeleted = await _storyService.DeleteStoryAsync(storyId);
        if(!isDeleted) return NotFound();

        return Ok();
    }


    [HttpPost("{storyId:int}/join")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MessageResponse>> JoinStory([FromRoute] int storyId)
    {
        string? loggedUser = User.FindFirstValue(ClaimTypes.Name);
        if (loggedUser is null) return Unauthorized();
        
        try
        {
            bool joinedStory = await _storyService.JoinStoryAsync(loggedUser, storyId);
            if (!joinedStory)
            {
                return Conflict();
            };
        
            return Ok();
        }
        catch (Exception ex) when (ex is UserNotFoundException || ex is StoryNotFoundException)
        {
            switch (ex)
            {
                case UserNotFoundException notFoundEx:
                    _logger.LogWarning(notFoundEx, "User {UserName} not found at story join", loggedUser);
                    break;
                case StoryNotFoundException storyNotFoundEx:
                    _logger.LogWarning(storyNotFoundEx, "Story {StoryId} not found at story join", storyId);
                    break;
            }
            
            return NotFound();
        }
    }

    [HttpPost("{storyId:int}/leave")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MessageResponse>> LeaveStory([FromRoute] int storyId)
    {
        string? loggedUser = User.FindFirstValue(ClaimTypes.Name);
        if (loggedUser is null) return Unauthorized();
        
        try
        {
            bool hasLeftStory = await _storyService.LeaveStoryAsync(loggedUser, storyId);
            if(!hasLeftStory) return Forbid();

            return Ok();
        }
        catch (Exception ex) when (ex is UserNotFoundException || ex is StoryNotFoundException)
        {
            switch (ex)
            {
                case UserNotFoundException notFoundEx:
                    _logger.LogWarning(notFoundEx, "User {UserName} not found when leaving story", loggedUser);
                    break;
                case StoryNotFoundException storyNotFoundEx:
                    _logger.LogWarning(storyNotFoundEx, "Story {StoryId} not found when leaving story", storyId);
                    break;
            }
            
            return NotFound();
        }
    }
    
    [AllowAnonymous]
    [HttpGet("{storyId:int}/story-parts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]    
    public async Task<ActionResult<CompleteStoryDto>> GetCompleteStory([FromRoute] int storyId)
    {
        CompleteStoryDto? completeStory = await _storyService.GetCompleteStoryAsync(storyId);

        if(completeStory is null) return NotFound();

        return Ok(completeStory);
    }
    
    [HttpDelete("{storyId:int}/story-parts/{storyPartId:int}")]
    [Authorize(Policy = PolicyConstants.RequiredAdminRole)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MessageResponse>> DeleteStoryPart([FromRoute] int storyId, [FromRoute] int storyPartId)
    {
        string? loggedUser = User.FindFirstValue(ClaimTypes.Name);
        if (loggedUser is null) return Unauthorized();

        bool isDeleted = await _storyService.DeleteStoryPart(storyId, storyPartId);
        if (!isDeleted) return NotFound();

        return Ok();
    }
}