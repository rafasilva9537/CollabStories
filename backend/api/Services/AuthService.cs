using api.Constants;
using api.Data;
using api.Dtos.AppUser;
using api.Dtos.Pagination;
using api.Exceptions;
using api.Interfaces;
using api.Mappers;
using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace api.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IImageService _imageService;
    public AuthService(
        ApplicationDbContext context, 
        UserManager<AppUser> userManager, 
        ITokenService tokenService,
        IDateTimeProvider dateTimeProvider,
        IImageService imageService)
    {
        _context = context;
        _userManager = userManager;
        _tokenService = tokenService;
        _dateTimeProvider = dateTimeProvider;
        _imageService = imageService;
    }

    public async Task<string> RegisterAsync(RegisterUserDto registerUserDto)
    {
        
        AppUser newUser = registerUserDto.ToAppUserModel();
        newUser.CreatedDate = _dateTimeProvider.UtcNow;
        newUser.SecurityStamp = Guid.NewGuid().ToString(); //TODO: see if it's necessary at user creation/registering

        IdentityResult resultUser = await _userManager.CreateAsync(newUser, registerUserDto.Password);
        if(!resultUser.Succeeded)
        {
            var errors = resultUser.Errors.ToList();
            UserRegistrationException exception = new("Invalid username, email or password.", errors);
            throw exception;
        }

        IdentityResult resultRole = await _userManager.AddToRoleAsync(newUser, RoleConstants.User);
        if(!resultRole.Succeeded)
        {
            var errors = resultUser.Errors.ToList();
            UserRegistrationException exception = new("Unable to add user to role.", errors);
            throw exception; 
        }

        string token = await _tokenService.GenerateToken(newUser);
        return token;
    }
    
    public async Task<string?> LoginAsync(LoginUserDto loginUserDto)
    {
        // TODO: change to use CheckPasswordSignInAsync(), eliminating one round trip
        AppUser? loggedUser = await _userManager.FindByNameAsync(loginUserDto.UserName);

        if(loggedUser is null) throw new UserNotFoundException("User does not exist on login attempt.");

        string password = loginUserDto.Password;
        if(!await _userManager.CheckPasswordAsync(loggedUser, password))
        {
            return null;
        }
        
        string token = await _tokenService.GenerateToken(loggedUser);
        return token;
    }
    
    public async Task<PagedKeysetList<UserMainInfoDto>> GetUsersAsync(
        DateTimeOffset? lastDate = null,
        string? lastUserName = null,
        int pageSize = 15)
    {
        var query = _context.AppUser.AsQueryable();
        
        if (lastUserName != null && lastDate.HasValue)
        {
            query = query.Where(u => 
                u.CreatedDate < lastDate || 
                (u.CreatedDate == lastDate && string.Compare(u.UserName, lastUserName) <= 0));
        }

        var usersDto = await query
            .OrderByDescending(au => au.CreatedDate)
            .ThenByDescending(au => au.UserName)
            .Take(pageSize + 1)
            .Select(AppUserMappers.ProjectToUserMainInfoDto)
            .ToListAsync();
        
        bool hasMore = usersDto.Count > pageSize;
        DateTimeOffset? nextDate = null;
        string? nextUserName = null;
        if (hasMore)
        {
            nextDate = usersDto[^1].CreatedDate;
            nextUserName = usersDto[^1].UserName;
            usersDto.RemoveAt(usersDto.Count - 1);
        }

        PagedKeysetList<UserMainInfoDto> keySetUsersList = new()
        {
            Data = usersDto,
            HasMore = hasMore,
            NextDate = nextDate,
            NextUserName = nextUserName
        };
        
        return keySetUsersList;
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
        bool descriptionIsChanged = updatedUserDto.Description != String.Empty;
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

    public async Task UpdateProfileImageAsync(string username, IFormFile image, string directoryName)
    {
        AppUser user = await _context.AppUser.FirstAsync(au => au.UserName == username);
        
        string savedImage = await _imageService.SaveImageAsync(image, directoryName);
        
        user.ProfileImage = savedImage;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteProfileImageAsync(string username, string directoryName)
    {
        AppUser user = await _context.AppUser.FirstAsync(au => au.UserName == username);

        _imageService.DeleteImage(user.ProfileImage, directoryName);

        user.ProfileImage = string.Empty;
        await _context.SaveChangesAsync();
    }
}