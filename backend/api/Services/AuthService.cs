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
    private readonly ILogger<AuthService> _logger;
    public AuthService(
        ApplicationDbContext context, 
        UserManager<AppUser> userManager, 
        ITokenService tokenService,
        IDateTimeProvider dateTimeProvider,
        IImageService imageService,
        ILogger<AuthService> logger)
    {
        _context = context;
        _userManager = userManager;
        _tokenService = tokenService;
        _dateTimeProvider = dateTimeProvider;
        _imageService = imageService;
        _logger = logger;
    }

    public async Task<string> RegisterAsync(RegisterUserDto registerUserDto)
    {
        
        AppUser newUser = registerUserDto.ToAppUserModel();
        newUser.CreatedDate = _dateTimeProvider.UtcNow;

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

    /// <summary>
    /// Authenticates a user based on the provided login credentials and returns a JWT token if the login is successful.
    /// </summary>
    /// <param name="loginUserDto">An object containing the user's login credentials, including username and password.</param>
    /// <returns>A JWT token as a string if the login is successful, otherwise, null if the provided credentials are invalid.</returns>
    /// <exception cref="UserNotFoundException">Thrown when the username does not exist.</exception>
    public async Task<string?> LoginAsync(LoginUserDto loginUserDto)
    {
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
    
    public async Task<PagedKeysetUserList<UserMainInfoDto>> GetUsersAsync(
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
            .AsNoTracking()
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

        PagedKeysetUserList<UserMainInfoDto> keySetUsersUserList = new()
        {
            Items = usersDto,
            HasMore = hasMore,
            NextDate = nextDate,
            NextUserName = nextUserName
        };
        
        return keySetUsersUserList;
    }

    public async Task DeleteByNameAsync(string username)
    {
        AppUser? userModel = await _userManager.FindByNameAsync(username);
        if(userModel is null) throw new UserNotFoundException("User does not exist on delete attempt.");
        
        await _userManager.DeleteAsync(userModel);
    }

    /// <summary>
    /// Retrieves the details of a user based on the provided username.
    /// </summary>
    /// <param name="username">The username of the user to retrieve.</param>
    /// <returns>An <see cref="AppUserDto"/> object representing the user's details if found; otherwise, null.</returns>
    public async Task<AppUserDto?> GetUserAsync(string username)
    {
        AppUser? appUserModel = await _userManager.FindByNameAsync(username);
        if(appUserModel is null) return null;
        return appUserModel.ToAppUserDto();
    }
    
    public async Task<AppUserDto> UpdateUserFieldsAsync(string userName, UpdateUserFieldsDto updateUserFieldsDto)
    {
        AppUser? updatedUser = await _context.AppUser
            .FirstOrDefaultAsync(au => au.UserName == userName);
        if (updatedUser is null) throw new UserNotFoundException("User does not exist on update attempt.");

        bool userNameIsNullOrEmpty = string.IsNullOrEmpty(updateUserFieldsDto.UserName);
        bool nickNameIsNullOrEmpty = string.IsNullOrEmpty(updateUserFieldsDto.NickName);
        bool emailIsNullOrEmpty = string.IsNullOrEmpty(updateUserFieldsDto.Email);
        bool descriptionIsNullOrEmpty = string.IsNullOrEmpty(updateUserFieldsDto.Description);
        
        if (userNameIsNullOrEmpty && emailIsNullOrEmpty && descriptionIsNullOrEmpty && nickNameIsNullOrEmpty)
        {
            _logger.LogWarning("User {UserName} tried to update fields without any field to update.", userName);
            throw new ArgumentException("At least one field must be updated.");
        }
        
        if (!userNameIsNullOrEmpty) updatedUser.UserName = updateUserFieldsDto.UserName!;
        if (!emailIsNullOrEmpty) updatedUser.Email = updateUserFieldsDto.Email!;
        if (!descriptionIsNullOrEmpty) updatedUser.Description = updateUserFieldsDto.Description!;
        if(!nickNameIsNullOrEmpty) updatedUser.NickName = updateUserFieldsDto.NickName!;
        
        IdentityResult updateResult = await _userManager.UpdateAsync(updatedUser);
        if (!updateResult.Succeeded)
        {
            var errors = updateResult.Errors.ToList();
            UserRegistrationException exception = new("Unable to update user fields.", errors);
            throw exception;
        }
        
        AppUserDto appUserDto = updatedUser.ToAppUserDto();
        return appUserDto;
    }

    public async Task ChangeUserPasswordAsync(string userName, ChangePasswordDto changePasswordDto)
    {
        AppUser? user = await _context.AppUser
            .FirstOrDefaultAsync(au => au.UserName == userName);
        if(user is null) throw new UserNotFoundException("User does not exist on password change attempt.");
        
        if (string.IsNullOrWhiteSpace(changePasswordDto.CurrentPassword) || 
            string.IsNullOrWhiteSpace(changePasswordDto.NewPassword))
        {
            _logger.LogWarning("User {UserName} tried to change password without providing a password.", userName);
            throw new ArgumentException("Passwords cannot be empty.");
        }
        
        IdentityResult changePasswordResult = await _userManager.ChangePasswordAsync(
            user,
            changePasswordDto.CurrentPassword, 
            changePasswordDto.NewPassword
        );

        if(!changePasswordResult.Succeeded)
        {
            var errors = changePasswordResult.Errors.ToList();
            UserPasswordException exception = new("User password change failed.", errors);
            throw exception;
        }
    }

    public async Task UpdateProfileImageAsync(string username, IFormFile image, string directoryName)
    {
        AppUser? user = await _context.AppUser
            .FirstOrDefaultAsync(au => au.UserName == username);
        if(user is null) throw new UserNotFoundException("User does not exist on profile image update attempt.");
        
        string savedImage = await _imageService.SaveImageAsync(image, directoryName);
        
        user.ProfileImage = savedImage;
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes the profile image of a user from the specified directory.
    /// </summary>
    /// <param name="username">The username of the user whose profile image is to be deleted.</param>
    /// <param name="directoryName">The name of the directory where the profile images are stored.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    /// <exception cref="UserNotFoundException">Thrown when the specified user does not exist.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the user's profile has no profile image.</exception>
    public async Task DeleteProfileImageAsync(string username, string directoryName)
    {
        AppUser? user = await _context.AppUser
            .FirstOrDefaultAsync(au => au.UserName == username);
        if(user is null) throw new UserNotFoundException("User does not exist on profile image delete attempt.");

        if (string.IsNullOrEmpty(user.ProfileImage))
        {
            throw new FileNotFoundException("Profile image not found to delete.");
        }
        
        _imageService.DeleteImage(user.ProfileImage, directoryName);

        user.ProfileImage = string.Empty;
        await _context.SaveChangesAsync();
    }

}