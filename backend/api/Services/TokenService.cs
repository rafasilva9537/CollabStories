using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using api.Interfaces;
using Microsoft.IdentityModel.Tokens;
using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;

namespace api.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<AppUser> _userManager;
    private readonly IDateTimeProvider _dateTimeProvider;

    public TokenService(IConfiguration configuration, UserManager<AppUser> userManager, IDateTimeProvider dateTimeProvider)
    {
        _configuration = configuration;
        _userManager = userManager;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<string> GenerateToken(AppUser user)
    {
        string? secret = _configuration["JwtConfig:Secret"] ?? Environment.GetEnvironmentVariable("JWT_SECRET");
        string? issuer = _configuration["JwtConfig:ValidIssuer"] ?? Environment.GetEnvironmentVariable("JWT_ISSUER");
        string? audience = _configuration["JwtConfig:ValidAudiences"] ?? Environment.GetEnvironmentVariable("JWT_AUDIENCE");
        if(secret is null || issuer is null || audience is null)
        {
            throw new ArgumentNullException("Jwt is not set in the configuration");
        }

        SymmetricSecurityKey signingKey = new(Encoding.UTF8.GetBytes(secret));
        JsonWebTokenHandler tokenHandler = new();

        var userRoles = await _userManager.GetRolesAsync(user);

        List<Claim> claims = [
            new Claim(ClaimTypes.NameIdentifier, user.UserName),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
        ];

        claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

        SecurityTokenDescriptor tokenDescriptor = new() {
            Subject = new ClaimsIdentity(claims),
            Issuer = issuer,
            Audience = audience,
            Expires = _dateTimeProvider.UtcNowDateTime.AddDays(7), // TODO: decrease time in production
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256),
            IssuedAt = _dateTimeProvider.UtcNowDateTime,
            NotBefore = _dateTimeProvider.UtcNowDateTime,
        };

        string token = tokenHandler.CreateToken(tokenDescriptor);
        return token;
    }
}