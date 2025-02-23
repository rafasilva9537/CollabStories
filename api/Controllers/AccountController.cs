using Microsoft.AspNetCore.Mvc;
using api.Dtos.AppUser;
using Microsoft.AspNetCore.Authorization;
using api.Services;
using Microsoft.AspNetCore.Identity;
using api.Models;

namespace api.Controllers;

[Authorize]
[ApiController]
[Route("accounts")]
public class AccountController : ControllerBase
{
    private readonly IAuthService _authService;
    public AccountController(IAuthService authService)
    {
        _authService = authService;
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
        RegisterResult registerResult = await _authService.RegisterAsync(registerUser);
        
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
        string? token = await _authService.LoginAsync(loginUser);

        if(token is null)
        {
            ModelState.AddModelError("Invalid Login", "Invalid username or password");
            return BadRequest(ModelState);
        }

        return Ok(token);
    }

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

    [HttpDelete("{username}/delete")]
    public async Task<IActionResult> DeleteUser([FromRoute] string username)
    {
        bool isDeleted = await _authService.DeleteByNameAsync(username);
        
        if(!isDeleted) return BadRequest("Impossible to delete user");
        return Ok("User was successfully deleted!");
    }
}