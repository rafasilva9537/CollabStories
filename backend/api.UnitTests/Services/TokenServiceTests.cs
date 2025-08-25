using System.Security.Claims;
using api.Interfaces;
using api.Services;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Xunit.Abstractions;
using Microsoft.AspNetCore.Identity;
using api.Models;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace api.UnitTests.Services;

public class TokenServiceTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly IConfiguration _configuration;
    private readonly UserManager<AppUser> _userManagerMock;
    private readonly IDateTimeProvider _dateTimeProvider;

    public TokenServiceTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;

        _configuration = Substitute.For<IConfiguration>();

        _configuration["JwtConfig:Secret"].Returns("not-really-secret-this-string-is-made-for-test");
        _configuration["JwtConfig:ValidIssuer"].Returns("http://localhost:5014");
        _configuration["JwtConfig:ValidAudiences"].Returns("http://localhost:5014");

        var userStoreMock = Substitute.For<IUserStore<AppUser>>();
        _userManagerMock = Substitute.For<UserManager<AppUser>>(userStoreMock, null, null, null, null, null, null, null, null);

        IList<string> rolesMock = ["User"];
        var asyncRolesMock = Task.FromResult(rolesMock);
        _userManagerMock.GetRolesAsync(Arg.Any<AppUser>()).Returns(asyncRolesMock);
        
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        DateTime fakeDate = new(2025, 10, 12, 0, 0, 0, DateTimeKind.Utc);
        _dateTimeProvider.UtcNowDateTime.Returns(fakeDate);
    }

    [Theory]
    [InlineData("test_user", "test-user@gmail.com")]
    [InlineData("jhon45-rt", "jhon-user@hotmail.com")]
    [InlineData("_dude_", "user@email.com")]
    public async Task GenerateToken_GivenValidAppUser_ReturnsTokenWithExpectedClaims(string expectedUserName, string expectedEmail)
    {
        // Arrange
        AppUser user = new()
        {
            UserName = expectedUserName,
            Email = expectedEmail
        };
        
        IDateTimeProvider? dateTimeProvider = Substitute.For<IDateTimeProvider>();
        DateTime fakeDate = new(2025, 10, 12, 0, 0, 0, DateTimeKind.Utc);
        dateTimeProvider.UtcNowDateTime.Returns(fakeDate);
        TokenService tokenService = new(_configuration, _userManagerMock, dateTimeProvider);

        JsonWebTokenHandler tokenHandler = new();
        
        // Act
        string tokenString = await tokenService.GenerateToken(user);
        
        // Assert
        JsonWebToken jwt = tokenHandler.ReadJsonWebToken(tokenString);
        Claim claimName = jwt.GetClaim(ClaimTypes.Name);
        Claim claimNameIdentifier = jwt.GetClaim(ClaimTypes.NameIdentifier);
        Claim claimEmail = jwt.GetClaim(ClaimTypes.Email);
        
        Assert.Equal(SecurityAlgorithms.HmacSha256, jwt.Alg);
        Assert.Equal("JWT", jwt.Typ);
        
        Assert.Equal(9, jwt.Claims.Count());
        Assert.Equal(expectedUserName, claimName.Value);
        Assert.Equal(expectedUserName, claimNameIdentifier.Value);
        Assert.Equal(expectedEmail, claimEmail.Value);
        Assert.Contains(_configuration["JwtConfig:ValidAudiences"], jwt.Audiences);
        Assert.Equal(_configuration["JwtConfig:ValidIssuer"], jwt.Issuer);
        Assert.Equal(fakeDate.AddDays(7), jwt.ValidTo);
        Assert.Equal(fakeDate, jwt.ValidFrom);
        Assert.Equal(fakeDate, jwt.IssuedAt);
    }
    
    [Fact]
    public async Task GenerateToken_ForTwoDifferentUsers_DoesNotReturnIdenticalTokens()
    {
        // Arrange
        AppUser firstAppUser = new()
        {
            UserName = "test_user1",
            Email = "user@example1.com"
        };

        AppUser secondAppUser = new()
        {
            UserName = "test_user2",
            Email = "user@example2.com"
        };

        TokenService tokenService = new(_configuration, _userManagerMock, _dateTimeProvider);

        // Act
        string firstToken = await tokenService.GenerateToken(firstAppUser);
        string secondToken = await tokenService.GenerateToken(secondAppUser);

        // Assert
        Assert.False(firstToken == secondToken);
    }
}