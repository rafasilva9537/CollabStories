using api.Dtos.AppUser;
using api.Dtos.Pagination;
using api.Exceptions;

namespace api.Interfaces;

public interface IAuthService
{
    Task<PagedKeysetUserList<UserMainInfoDto>> GetUsersAsync(DateTimeOffset? lastDate = null, string? lastUserName = null, int pageSize = 15);
    
    Task<string> RegisterAsync(RegisterUserDto registerUserDto);
    
    /// <summary>
    /// Authenticates a user based on the provided login details.
    /// </summary>
    /// <param name="loginUserDto">An object containing the user's login credentials including username and password.</param>
    /// <returns>
    /// A string representing a JWT token if the authentication is successful; otherwise, null if the password is incorrect.
    /// </returns>
    /// <exception cref="UserNotFoundException">Thrown when the user does not exist in the database.</exception>
    Task<string?> LoginAsync(LoginUserDto loginUserDto);
    
    Task DeleteByNameAsync(string username);
    
    Task<AppUserDto?> GetUserAsync(string username);
    
    Task UpdateProfileImageAsync(string username, IFormFile image, string directoryName);
    
    Task DeleteProfileImageAsync(string username, string directoryName);
    
    Task<AppUserDto> UpdateUserFieldsAsync(string userName, UpdateUserFieldsDto updateUserFieldsDto);
    
    Task ChangeUserPasswordAsync(string userName, ChangePasswordDto changePasswordDto);
}