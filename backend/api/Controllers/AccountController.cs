using Microsoft.AspNetCore.Mvc;
using api.Dtos.AppUser;
using Microsoft.AspNetCore.Authorization;
using api.Constants;
using System.Security.Claims;
using api.Dtos.HttpResponses;
using api.Dtos.Pagination;
using api.Exceptions;
using api.Interfaces;

namespace api.Controllers;

[Authorize(Roles = RoleConstants.User)]
[ApiController]
[Route("accounts")]
public class AccountController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AccountController> _logger;
    
    public AccountController(IAuthService authService, ILogger<AccountController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IList<UserMainInfoDto>>> GetUsers(
        [FromQuery] DateTimeOffset? lastDate, 
        [FromQuery] string? lastUserName)
    {
        const int pageSize = 15;
        PagedKeysetUserList<UserMainInfoDto> pagedUsers = await _authService.GetUsersAsync(lastDate, lastUserName, pageSize);
        return Ok(pagedUsers);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<TokenResponse>> Register([FromBody] RegisterUserDto registerUser)
    {
        try
        {
            string token = await _authService.RegisterAsync(registerUser);
            _logger.LogInformation("User '{UserName}' registered at {RegisterTime}", registerUser.UserName, DateTimeOffset.UtcNow);
            return Ok(new TokenResponse { Token = token });
        }
        catch (UserRegistrationException ex)
        {
            _logger.LogError(ex, "User '{UserName}' registration failed at {RegisterTime}", registerUser.UserName, DateTimeOffset.UtcNow);
            var errors = ex.Errors?.ToDictionary();

            ValidationProblemDetails problemDetails = new(errors ?? []);
            return ValidationProblem(problemDetails);
        }
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<TokenResponse>> Login([FromBody] LoginUserDto loginUser)
    {
        // TODO: remove user
        var user = await _authService.GetUserAsync(loginUser.UserName);
        string? token = await _authService.LoginAsync(loginUser); // TODO: remove

        if(token is null)
        {
            ModelState.AddModelError("Invalid Login", "Invalid username or password");
            return BadRequest(ModelState);
        }

        // TODO: remove username
        return Ok(new TokenResponse { Token = token }); 
    }
    
    // TODO: return less detailed user data if not the logged user or admin
    [HttpGet("{username}")]
    public async Task<ActionResult<AppUserDto>> GetUser([FromRoute] string username)
    {
        AppUserDto? appUser = await _authService.GetUserAsync(username);

        if(appUser is null) return NotFound();
        
        return Ok(appUser);
    }
    
    [HttpPut("{username}/update")]
    public async Task<ActionResult<AppUserDto>> UpdateUser([FromBody] UpdateUserDto updateUserData)
    {
        AppUserDto updatedUser = await _authService.UpdateUserAsync(updateUserData);
        return CreatedAtAction(nameof(GetUser), new { username = updatedUser.UserName }, updatedUser);
    } 

    [Authorize(Policy = PolicyConstants.RequiredAdminRole)]
    [HttpDelete("{username}/delete")]
    public async Task<ActionResult<MessageResponse>> DeleteUser([FromRoute] string username)
    {
        bool isDeleted = await _authService.DeleteByNameAsync(username);
        
        if(!isDeleted) return BadRequest("Impossible to delete user");
        return Ok(new MessageResponse { Message = "User was successfully deleted!" });
    }

    [HttpPut("{username}/profile-image")]
    public async Task<ActionResult> UpdateProfileImage(IFormFile image)
    {
        string? loggedUser = User.FindFirstValue(ClaimTypes.Name);

        if(loggedUser is null)
        {
            return Forbid();
        }

        await _authService.UpdateProfileImageAsync(loggedUser, image, DirectoryPathConstants.ProfileImages);

        return Ok();
    }

    // TODO: remove username in route parameter?
    [HttpDelete("{username}/profile-image")]
    public async Task<ActionResult> DeleteProfileImage()
    {
        string? loggedUser = User.FindFirstValue(ClaimTypes.Name);

        if(loggedUser is null)
        {
            return Forbid();
        }

        await _authService.DeleteProfileImageAsync(loggedUser, DirectoryPathConstants.ProfileImages);

        return Ok();
    }
}