using api.Dtos.AppUser;
using api.Services;

namespace api.Interfaces;

public interface IAuthService
{
    Task<IList<UserMainInfoDto>> GetUsersAsync(int? lastId);
    Task<AuthenticationResult> RegisterAsync(RegisterUserDto registerUserDto);
    Task<string?> LoginAsync(LoginUserDto loginUserDto);
    Task<bool> DeleteByNameAsync(string username);
    Task<AppUserDto?> GetUserAsync(string username);
    Task<AppUserDto> UpdateUserAsync(UpdateUserDto updateUserDto);
    Task UpdateProfileImageAsync(string username, IFormFile image, string directoryName);
    Task DeleteProfileImageAsync(string username, string directoryName);
}