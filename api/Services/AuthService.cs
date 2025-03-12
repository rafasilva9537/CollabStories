using api.Constants;
using api.Data;
using api.Dtos.AppUser;
using api.Mappers;
using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace api.Services;

public struct AuthenticationResult
{
    public string Token { get; init; }
    public bool Succeeded { get; init; }
    public IEnumerable<string> ErrorMessages { get; set; }
}

public interface IAuthService
{
    Task<IList<UserMainInfoDto>> GetUsersAsync();
    Task<AuthenticationResult> RegisterAsync(RegisterUserDto registerUserDto);
    Task<string?> LoginAsync(LoginUserDto loginUserDto);
    Task<bool> DeleteByNameAsync(string username);
    Task<AppUserDto?> GetUserAsync(string username);
    Task<AppUserDto> UpdateUserAsync(UpdateUserDto updateUserDto);
}

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;
    public AuthService(ApplicationDbContext context, UserManager<AppUser> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _context = context;
    }

    public async Task<AuthenticationResult> RegisterAsync(RegisterUserDto registerUserDto)
    {
        AppUser newUser = registerUserDto.ToAppUserModel();
        newUser.SecurityStamp = Guid.NewGuid().ToString(); //TODO: see if it's necessary at user creation/registering

        IdentityResult resultUser = await _userManager.CreateAsync(newUser, registerUserDto.Password);

        if(!resultUser.Succeeded)
        {
            return new AuthenticationResult { Succeeded = false, ErrorMessages = resultUser.Errors.Select(e => e.Description) };
        }

        var resultRole = await _userManager.AddToRoleAsync(newUser, RoleConstants.User);
        if(!resultRole.Succeeded)
        {
            return new AuthenticationResult { Succeeded = false, ErrorMessages = resultRole.Errors.Select(e => e.Description) };
        }

        string token = await _tokenService.GenerateToken(newUser);
        return new AuthenticationResult { Succeeded = true, Token = token};
    }

    public async Task<string?> LoginAsync(LoginUserDto loginUserDto)
    {
        // TODO: change to use CheckPasswordSignInAsync(), eliminating one round trip
        AppUser? loggedUser = await _userManager.FindByNameAsync(loginUserDto.UserName);

        if(loggedUser is null) return null;

        string password = loginUserDto.Password;
        if(!await _userManager.CheckPasswordAsync(loggedUser, password))
        {
            return null;
        }
        
        string token = await _tokenService.GenerateToken(loggedUser);
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