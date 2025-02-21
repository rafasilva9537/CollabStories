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
}