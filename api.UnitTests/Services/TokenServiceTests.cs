using api.Services;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Xunit.Abstractions;
using Microsoft.AspNetCore.Identity;
using api.Models;

namespace api.UnitTests.Services;

public class TokenServiceTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly IConfiguration _configuration;
    private readonly UserManager<AppUser> _userManagerMock;

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
    }
    
    [Fact]
    public async void GenerateToken_ForTwoDifferentUsers_DoesNotReturnIdenticalTokens()
    {
        // Arrange
        AppUser firstAppUser = new AppUser();
        firstAppUser.UserName = "test_user1";
        firstAppUser.Email = "user@example1.com";

        AppUser secondAppUser = new AppUser();
        secondAppUser.UserName = "test_user2";
        secondAppUser.Email = "user@example2.com";

        TokenService tokenService = new TokenService(_configuration, _userManagerMock);

        // Act
        string firstToken = await tokenService.GenerateToken(firstAppUser);
        string secondToken = await tokenService.GenerateToken(secondAppUser);

        // Assert
        Assert.False(firstToken == secondToken);
    }
}