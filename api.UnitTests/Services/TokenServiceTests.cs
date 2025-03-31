using api.Services;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Bogus;
using Xunit.Abstractions;
using Microsoft.AspNetCore.Identity;
using api.Models;

namespace api.UnitTests.Services;

public class TokenServiceTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly IConfiguration _configuration;
    private readonly UserManager<AppUser> _userManagerMock;
    private readonly Faker _faker = new Faker("pt_BR");

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
    public async void GenerateToken_CreatingTwoDifferentTokens_DoesNotReturnIdenticalTokens()
    {
        // Arrange
        AppUser appUser = new AppUser();
        appUser.UserName = "test_user";
        appUser.Email = "user@example.com";

        TokenService tokenService = new TokenService(_configuration, _userManagerMock);

        // Act
        string firstToken = await tokenService.GenerateToken(appUser);
        string secondToken = await tokenService.GenerateToken(appUser);

        // Assert
        Assert.False(firstToken == secondToken);
    }
}