
using api.Data;
using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using api.Dtos.AppUser;
using api.Mappers;
using Microsoft.EntityFrameworkCore;

namespace api.Services;

public struct RegisterResult
{
    public string Token { get; init; }
    public bool Succeeded { get; init; }
    public IEnumerable<string> ErrorMessages { get; set; }
}

public interface IUserManagerExtension
{
    Task<IList<UserMainInfoDto>> GetUsersAsync();
    Task<RegisterResult> RegisterAsync(RegisterUserDto registerUserDto);
    Task<string?> LoginAsync(LoginUserDto loginUserDto);
    Task<bool> DeleteByNameAsync(string username);
    Task<AppUserDto?> GetUserAsync(string username);
    Task<AppUserDto> UpdateUserAsync(UpdateUserDto updateUserDto);
}

public class AppUserManager : UserManager<AppUser>, IUserManagerExtension
{
    private readonly ApplicationDbContext _context;
    private readonly ITokenService _tokenService;

    public AppUserManager(ApplicationDbContext context, ITokenService tokenService, IUserStore<AppUser> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<AppUser> passwordHasher, IEnumerable<IUserValidator<AppUser>> userValidators, IEnumerable<IPasswordValidator<AppUser>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<AppUser>> logger) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
        _context = context;
        _tokenService = tokenService;
    }


    public async Task<RegisterResult> RegisterAsync(RegisterUserDto registerUserDto)
    {
        AppUser? existedUser = await FindByNameAsync(registerUserDto.UserName);

        AppUser newUser = registerUserDto.ToAppUserModel();
        newUser.SecurityStamp = Guid.NewGuid().ToString(); //TODO: see if it's necessary at user creation/registering
        IdentityResult result = await CreateAsync(newUser, registerUserDto.Password);

        if(!result.Succeeded)
        {
            return new RegisterResult { Succeeded = true, ErrorMessages = result.Errors.Select(e => e.Description) };
        }

        string token = _tokenService.GenerateToken(newUser);
        return new RegisterResult { Succeeded = true, Token = token};
    }

    public async Task<string?> LoginAsync(LoginUserDto loginUserDto)
    {
        AppUser? loggedUser = await FindByNameAsync(loginUserDto.UserName);

        if(loggedUser is null) return null;

        string password = loginUserDto.Password;
        if(!await CheckPasswordAsync(loggedUser, password))
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

    public async Task<bool> DeleteByNameAsync(string username)
    {
        AppUser? userModel = await FindByNameAsync(username);
        
        if(userModel is null) return false;
        
        await DeleteAsync(userModel);
        return true;
    }

    public async Task<AppUserDto?> GetUserAsync(string username)
    {
        AppUser? appUserModel = await FindByNameAsync(username);
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
            await ChangePasswordAsync(updatedUser, currentPassword, newPassword);
        }
        if(userNameIsChanged) updatedUser.UserName = updatedUser.UserName;
        if(descriptionIsChanged) updatedUser.Description = updatedUserDto.Description;
        if(emailIsChanged) updatedUser.Email = updatedUserDto.Email;

        await UpdateUserAsync(updatedUser);
        return updatedUser.ToAppUserDto();
    }
}