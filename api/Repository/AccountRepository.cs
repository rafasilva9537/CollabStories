using Microsoft.AspNetCore.Identity;
using api.Models;
using api.Dtos.AppUser;
using api.Mappers;
using api.Services;
using api.Data;
using Microsoft.EntityFrameworkCore;

namespace api.Repository;

public interface IAccountRepository
{
    Task<IList<UserMainInfoDto>> GetUsersAsync();
    Task<RegisterResult> RegisterAsync(RegisterUserDto registerUserDto);
    Task<string?> LoginAsync(LoginUserDto loginUserDto);
    Task<bool> DeleteUserAsync(string username);
    Task<AppUserDto?> GetUserAsync(string username);
}

public struct RegisterResult
{
    public string Token { get; init; }
    public bool Succeeded { get; init; }
    public IEnumerable<string> ErrorMessages { get; set; }
}

public class AccountRepository : IAccountRepository
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;
    public AccountRepository(ApplicationDbContext context, UserManager<AppUser> userManager, ITokenService tokenService)
    {
        _context = context;
        _userManager = userManager;
        _tokenService = tokenService;
    }


    public async Task<RegisterResult> RegisterAsync(RegisterUserDto registerUserDto)
    {
        //TODO: maybe a better way to represent errors?
        AppUser? existedUser = await _userManager.FindByNameAsync(registerUserDto.UserName);

        if(existedUser is not null)
        {
            return new RegisterResult { Succeeded = false, ErrorMessages = new List<string> { "User already exists" }};
        }

        AppUser newUser = registerUserDto.ToAppUserModel();
        newUser.SecurityStamp = Guid.NewGuid().ToString(); //TODO: see if it's necessary at user creation/registering
        IdentityResult result = await _userManager.CreateAsync(newUser, registerUserDto.Password);

        if(!result.Succeeded)
        {
            return new RegisterResult { Succeeded = true, ErrorMessages = result.Errors.Select(e => e.Description) };
        }

        string token = _tokenService.GenerateToken(newUser);
        return new RegisterResult { Succeeded = true, Token = token};
    }

    public async Task<string?> LoginAsync(LoginUserDto loginUserDto)
    {
        AppUser? loggedUser = await _userManager.FindByNameAsync(loginUserDto.UserName);

        if(loggedUser is null) return null;

        string password = loginUserDto.Password;
        if(!await _userManager.CheckPasswordAsync(loggedUser, password))
        {
            return null;
        }
        
        string token = _tokenService.GenerateToken(loggedUser);
        return token;
    }

    public async Task<IList<UserMainInfoDto>> GetUsersAsync()
    {
        var usersDto = await _context.AppUser.Select(AppUserMappers.ProjetToUserMainInfoDto).ToListAsync();
        return usersDto;
    }

    public async Task<bool> DeleteUserAsync(string username)
    {
        AppUser? user = await _userManager.FindByNameAsync(username);
        
        if(user is null) return false;
        
        await _userManager.DeleteAsync(user);
        return true;
    }

    public async Task<AppUserDto?> GetUserAsync(string username)
    {
        AppUser? appUserModel = await _userManager.FindByNameAsync(username);
        if(appUserModel is null) return null;
        return appUserModel.ToAppUserDto();
    }
}