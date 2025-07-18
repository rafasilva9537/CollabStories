using api.Models;

namespace api.Interfaces;

public interface ITokenService
{
    Task<string> GenerateToken(AppUser user);
}