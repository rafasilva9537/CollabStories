using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace api.IntegrationTests;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string AuthenticationScheme = "TestScheme";
    public const string NameHeader = "Name";
    public const string NameIdentifierHeader = "NameIdentifier";
    public const string EmailHeader = "Email";
    public const string RoleHeader = "Role";
    
    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
    {
    }

    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        List<Claim> claims = [];

        if (Context.Request.Headers.TryGetValue(NameHeader, out StringValues userName))
        {
            claims.Add(new Claim(ClaimTypes.Name, userName[0]!));
        }

        if (Context.Request.Headers.TryGetValue(NameIdentifierHeader, out StringValues nameIdentifier))
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, nameIdentifier[0]!));
        }

        if (Context.Request.Headers.TryGetValue(EmailHeader, out StringValues email))
        {
            claims.Add(new Claim(ClaimTypes.Email, email[0]!));
        }

        if (Context.Request.Headers.TryGetValue(RoleHeader, out StringValues roles))
        {
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        }
        
        ClaimsIdentity identity = new(claims, AuthenticationScheme);
        ClaimsPrincipal principal = new(identity);
        AuthenticationTicket ticket = new(principal, AuthenticationScheme);;
        AuthenticateResult result = AuthenticateResult.Success(ticket);
        return Task.FromResult(result);
    }
}