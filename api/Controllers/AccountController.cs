using Microsoft.AspNetCore.Mvc;
using api.Dtos.AppUser;
using Microsoft.AspNetCore.Authorization;
using api.Services;
using api.Constants;
using System.Security.Claims;

namespace api.Controllers;

[Authorize(Roles = RoleConstants.User)]
[ApiController]
[Route("accounts")]
public class AccountController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IImageService _imageService;
    public AccountController(IAuthService authService, IImageService imageService)
    {
        _authService = authService;
        _imageService = imageService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        IList<UserMainInfoDto> users = await _authService.GetUsersAsync();
        return Ok(users);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto registerUser)
    {
        // TODO: remove user
        var user = await _authService.GetUserAsync(registerUser.UserName);
        AuthenticationResult registerResult = await _authService.RegisterAsync(registerUser);
        
        string token = registerResult.Token;

        if(registerResult.ErrorMessages is not null)
        {
            foreach(string errorDescription in registerResult.ErrorMessages)
            {
                ModelState.AddModelError("Error Message", errorDescription);
            }
            return BadRequest(ModelState);
        }

        return Ok(new { token }); 
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserDto loginUser)
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
        return Ok(new { token }); 
    }

    [Authorize(Policy = PolicyConstants.RequiredAdminRole)]
    [HttpGet("{username}")]
    public async Task<IActionResult> GetUser([FromRoute] string username)
    {
        AppUserDto? appUser = await _authService.GetUserAsync(username);

        if(appUser is null) return NotFound();
        
        return Ok(appUser);
    }
    
    [HttpPut("{username}/update")]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto updateUserData)
    {
        AppUserDto updatedUser = await _authService.UpdateUserAsync(updateUserData);
        return CreatedAtAction(nameof(GetUser), new { username = updatedUser.UserName }, updatedUser);
    } 

    [Authorize(Policy = PolicyConstants.RequiredAdminRole)]
    [HttpDelete("{username}/delete")]
    public async Task<IActionResult> DeleteUser([FromRoute] string username)
    {
        bool isDeleted = await _authService.DeleteByNameAsync(username);
        
        if(!isDeleted) return BadRequest("Impossible to delete user");
        return Ok("User was successfully deleted!");
    }

    [HttpPut("{username}/profile-image")]
    public async Task<IActionResult> UpdateProfileImage(IFormFile image)
    {
        string? loggedUser = User.FindFirstValue(ClaimTypes.Name);

        if(loggedUser is null)
        {
            return Forbid();
        }

        await _authService.UpdateProfileImageAsync(loggedUser, image, "ProfileImage");

        return Ok();
    }

    // TODO: remove username in route parameter?
    [HttpDelete("{username}/profile-image")]
    public async Task<IActionResult> DeleteProfileImage()
    {
        string? loggedUser = User.FindFirstValue(ClaimTypes.Name);

        if(loggedUser is null)
        {
            return Forbid();
        }

        await _authService.DeleteProfileImageAsync(loggedUser, "ProfileImage");

        return Ok();
    }
}