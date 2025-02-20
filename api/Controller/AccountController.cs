using Microsoft.AspNetCore.Mvc;
using api.Repository;
using api.Dtos.AppUser;

namespace api.Controller;

[ApiController]
[Route("account")]
public class AccountController : ControllerBase
{
    private readonly IAccountRepository _accountRepository;
    public AccountController(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto registerUser)
    {
        var registerResult = await _accountRepository.RegisterAsync(registerUser);
        
        var token = registerResult.Token;

        if(registerResult.Errors is not null)
        {
            foreach(var errorDescription in registerResult.Errors)
            {
                ModelState.AddModelError(String.Empty, errorDescription);
            }
            return BadRequest(ModelState);
        }

        if(token == null) return StatusCode(500, new { Message = "Server was unable to create token." });

        return Ok(new { token });
    }
}