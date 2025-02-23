using api.Data;
using api.Dtos.AppUser;
using api.Mappers;
using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace api.Services;

public struct RegisterResult
{
    public string Token { get; init; }
    public bool Succeeded { get; init; }
    public IEnumerable<string> ErrorMessages { get; set; }
}

public interface IAuthService
{
    Task<IList<UserMainInfoDto>> GetUsersAsync();
    Task<RegisterResult> RegisterAsync(RegisterUserDto registerUserDto);
    Task<string?> LoginAsync(LoginUserDto loginUserDto);
    Task<bool> DeleteByNameAsync(string username);
    Task<AppUserDto?> GetUserAsync(string username);
    Task<AppUserDto> UpdateUserAsync(UpdateUserDto updateUserDto);
}

public class AuthService : IAuthService
{
    // TODO: remove db context and pass to repository
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;
    public AuthService(ApplicationDbContext context, UserManager<AppUser> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _context = context;
    }

    public async Task<RegisterResult> RegisterAsync(RegisterUserDto registerUserDto)
    {
        AppUser? existedUser = await _userManager.FindByNameAsync(registerUserDto.UserName);

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
        // Use UserManager to get users here
        var usersDto = await _context.AppUser.Select(AppUserMappers.ProjetToUserMainInfoDto).ToListAsync();
        return usersDto;
    }

    public async Task<bool> DeleteByNameAsync(string username)
    {
        AppUser? userModel = await _userManager.FindByNameAsync(username);
        
        if(userModel is null) return false;
        
        await _userManager.DeleteAsync(userModel);
        return true;
    }

    public async Task<AppUserDto?> GetUserAsync(string username)
    {
        AppUser? appUserModel = await _userManager.FindByNameAsync(username);
        if(appUserModel is null) return null;
        return appUserModel.ToAppUserDto();
    }

    public async Task<AppUserDto> UpdateUserAsync(UpdateUserDto updatedUserDto)
    {
        AppUser updatedUser = await _context.AppUser.FirstAsync(au => au.UserName == updatedUserDto.UserName);

        string currentPassword = updatedUserDto.CurrentPassword;
        string newPassword = updatedUserDto.NewPassword;
        bool passwordIsChanged = currentPassword != String.Empty && newPassword != String.Empty;

        bool userNameIsChanged = updatedUserDto.UserName != String.Empty;
        bool descriptionIsChanged = updatedUserDto.UserName != String.Empty;
        bool emailIsChanged = updatedUserDto.Email != String.Empty;

        if(passwordIsChanged)
        {
            await _userManager.ChangePasswordAsync(updatedUser, currentPassword, newPassword);
        }
        if(userNameIsChanged) updatedUser.UserName = updatedUser.UserName;
        if(descriptionIsChanged) updatedUser.Description = updatedUserDto.Description;
        if(emailIsChanged) updatedUser.Email = updatedUserDto.Email;

        await _userManager.UpdateAsync(updatedUser);
        return updatedUser.ToAppUserDto();
    }
}