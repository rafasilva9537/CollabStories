using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using api.Models;

namespace api.Services;

public interface ITokenService
{
    string GenerateToken(AppUser user);
}

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(AppUser user)
    {
        String? secret = _configuration["JwtConfig:Secret"];
        String? issuer = _configuration["JwtConfig:ValidIssuer"];
        String? audience = _configuration["JwtConfig:ValidAudiences"];
        if(secret is null || issuer is null || audience is null)
        {
            throw new ApplicationException("Jwt is not set in the configuration");
        }

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var tokenHandler = new JwtSecurityTokenHandler();

        // TODO: remove possible null reference of user properties
        Claim[] claims = [
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Name, user.Email),
        ];

        var tokenDescriptor = new SecurityTokenDescriptor {
            Subject = new ClaimsIdentity(claims),
            Issuer = issuer,
            Audience = audience,
            Expires = DateTime.UtcNow.AddHours(6),
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature)
        };

        SecurityToken securityToken = tokenHandler.CreateToken(tokenDescriptor);

        string token = tokenHandler.WriteToken(securityToken);
        return token;
    }
}