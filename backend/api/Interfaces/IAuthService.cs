using api.Dtos.AppUser;
using api.Dtos.Pagination;
using api.Exceptions;

namespace api.Interfaces;

public interface IAuthService
{
    Task<PagedKeysetList<UserMainInfoDto>> GetUsersAsync(DateTimeOffset? lastDate = null, string? lastUserName = null, int pageSize = 15);
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
    
    Task<bool> DeleteByNameAsync(string username);
    Task<AppUserDto?> GetUserAsync(string username);
    Task<AppUserDto> UpdateUserAsync(UpdateUserDto updateUserDto);
    Task UpdateProfileImageAsync(string username, IFormFile image, string directoryName);
    Task DeleteProfileImageAsync(string username, string directoryName);
}