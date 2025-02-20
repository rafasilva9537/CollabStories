using Microsoft.AspNetCore.Identity;
using api.Models;
using api.Dtos.AppUser;
using api.Mappers;
using api.Services;

namespace api.Repository;

public interface IAccountRepository
{
    Task<RegisterResult> RegisterAsync(RegisterUserDto registerUser);
}

public struct RegisterResult
{
    public string Token { get; init; }
    public bool Succeeded { get; init; }
    public IEnumerable<string>? Errors { get; set; }
}

public class AccountRepository : IAccountRepository
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;
    public AccountRepository(UserManager<AppUser> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<RegisterResult> RegisterAsync(RegisterUserDto registerUserDto)
    {
        //TODO: maybe a better way to represent errors?
        AppUser? existedUser = await _userManager.FindByNameAsync(registerUserDto.UserName);

        if(existedUser is not null)
        {
            return new RegisterResult { Succeeded = false, Errors = new List<string> { "User already exists" }};
        }

        AppUser newUser = registerUserDto.ToAppUserModel();
        newUser.SecurityStamp = Guid.NewGuid().ToString(); //TODO: see if it's necessary at user creation/registering
        var result = await _userManager.CreateAsync(newUser, registerUserDto.Password);

        if(!result.Succeeded)
        {
            return new RegisterResult { Succeeded = true, Errors = result.Errors.Select(e => e.Description) };
        }

        string token = _tokenService.GenerateToken(newUser);
        return new RegisterResult { Succeeded = true, Token = token};
    }
}