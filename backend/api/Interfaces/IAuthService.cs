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
    
    Task DeleteByNameAsync(string userName);

    /// <summary>
    /// Retrieves the public user information for a given username.
    /// </summary>
    /// <param name="userName">The username of the user whose public information is to be retrieved.</param>
    /// <returns>
    /// An instance of <see cref="PublicAppUserDto"/> containing the user's public information, such as nickname,
    /// or null if the user does not exist.
    /// </returns>
    Task<PublicAppUserDto?> GetPublicUserAsync(string userName);

    /// <summary>
    /// Retrieves private user details for a specified username.
    /// </summary>
    /// <param name="userName">The username of the user whose private details are being retrieved.</param>
    /// <returns>
    /// A <see cref="PrivateAppUserDto"/> object containing the private details of the user if found,
    /// or null if the user does not exist.
    /// </returns>
    Task<PrivateAppUserDto?> GetPrivateUserAsync(string userName);
    
    Task UpdateProfileImageAsync(string userName, IFormFile image, string directoryName);
    
    Task DeleteProfileImageAsync(string userName, string directoryName);
    
    Task<PublicAppUserDto> UpdateUserFieldsAsync(string userName, UpdateUserFieldsDto updateUserFieldsDto);
    
    Task ChangeUserPasswordAsync(string userName, ChangePasswordDto changePasswordDto);
}