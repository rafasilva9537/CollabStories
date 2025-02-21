using Microsoft.AspNetCore.Mvc;
using api.Repository;
using api.Dtos.AppUser;
using Microsoft.AspNetCore.Authorization;

namespace api.Controller;

[Authorize]
[ApiController]
[Route("accounts")]
public class AccountController : ControllerBase
{
    private readonly IAccountRepository _accountRepository;
    public AccountController(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        IList<UserMainInfoDto> users = await _accountRepository.GetUsersAsync();
        return Ok(users);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto registerUser)
    {
        RegisterResult registerResult = await _accountRepository.RegisterAsync(registerUser);
        
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
        string? token = await _accountRepository.LoginAsync(loginUser);

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
        AppUserDto? appUser = await _accountRepository.GetUserAsync(username);

        if(appUser is null) return NotFound();
        
        return Ok(appUser);
    }
    
    [HttpDelete("{username}/delete")]
    public async Task<IActionResult> DeleteUser([FromRoute] string username)
    {
        bool isDeleted = await _accountRepository.DeleteUserAsync(username);
        
        if(!isDeleted) return BadRequest("Impossible to delete user");
        return Ok("User was successfully deleted!");
    }
}