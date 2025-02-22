using Microsoft.AspNetCore.Mvc;
using api.Dtos.AppUser;
using Microsoft.AspNetCore.Authorization;
using api.Services;

namespace api.Controller;

[Authorize]
[ApiController]
[Route("accounts")]
public class AccountController : ControllerBase
{
    private readonly AppUserManager _userManager;
    public AccountController(AppUserManager userManager)
    {
        _userManager = userManager;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        IList<UserMainInfoDto> users = await _userManager.GetUsersAsync();
        return Ok(users);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto registerUser)
    {
        RegisterResult registerResult = await _userManager.RegisterAsync(registerUser);
        
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
        string? token = await _userManager.LoginAsync(loginUser);

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
        AppUserDto? appUser = await _userManager.GetUserAsync(username);

        if(appUser is null) return NotFound();
        
        return Ok(appUser);
    }
    
    [HttpPut("{username}/update")]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto updateUserData)
    {
        AppUserDto updatedUser = await _userManager.UpdateUserAsync(updateUserData);
        return CreatedAtAction(nameof(GetUser), new { username = updatedUser.UserName }, updatedUser);
    } 

    [HttpDelete("{username}/delete")]
    public async Task<IActionResult> DeleteUser([FromRoute] string username)
    {
        bool isDeleted = await _userManager.DeleteByNameAsync(username);
        
        if(!isDeleted) return BadRequest("Impossible to delete user");
        return Ok("User was successfully deleted!");
    }
}