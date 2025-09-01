using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using api.Constants;
using api.Dtos.AppUser;
using api.IntegrationTests.Constants;
using api.IntegrationTests.ControllersTests.ControllersFixtures;
using api.IntegrationTests.WebAppFactories;
using api.Models;

namespace api.IntegrationTests.ControllersTests;

[Collection(CollectionConstants.IntegrationTestsDatabase)]
public class AccountControllerTests : IClassFixture<AccountControllerFixture>
{
    private readonly AuthHandlerWebAppFactory _factory;
    private readonly AppUser _defaultUser;
    
    public AccountControllerTests(AccountControllerFixture fixture)
    {
        _factory = fixture.Factory;
        _defaultUser = fixture.DefaultUser;
    }

    [Theory]
    [InlineData(100, 15, false)]
    [InlineData(50, 15, false)]
    [InlineData(45, 15, false)]
    [InlineData(5, 4, true)]
    [InlineData(16, 15, true)]
    [InlineData(-15, 0, false)]
    [InlineData(0, 0, false)]
    [InlineData(null, 15, false)]
    public async Task GetUsers_GivenValidLastId_ReturnExpectedUsers(int? lastId, int expectedUsersCount, bool containsUserWithId1)
    {
        // Arrange
        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage? response = await client.GetAsync($"/accounts?lastId={lastId}");
        
        // Assert
        MediaTypeHeaderValue? contentType = response.Content.Headers.ContentType;
        
        response.EnsureSuccessStatusCode();
        Assert.NotNull(contentType);
        Assert.Equal("utf-8", contentType.CharSet);
        Assert.Equal("application/json", contentType.MediaType);

        var users = await response.Content.ReadFromJsonAsync<IList<UserMainInfoDto>>();
        
        Assert.NotNull(users);
        Assert.Equal(expectedUsersCount, users.Count);
        Assert.Equal(containsUserWithId1, users.Any(u => u.Id == 1));
    }
    
    [Fact]
    public async Task GetUser_WhenAuthenticated_ReturnsExpectedUser()
    {
        // Arrange
        AppUser expectedUser = _defaultUser;
        HttpClient client = _factory.CreateClientWithAuth(
            expectedUser.UserName,
            expectedUser.UserName,
            expectedUser.Email,
            RoleConstants.User
        );
        
        // Act
        HttpResponseMessage response = await client.GetAsync($"/accounts/{TestConstants.DefaultUserName}");
        
        // Assert
        MediaTypeHeaderValue? contentType = response.Content.Headers.ContentType;
        
        response.EnsureSuccessStatusCode();
        Assert.NotNull(contentType);
        Assert.Equal(MediaTypeNames.Application.Json, contentType.MediaType);
        Assert.Equal("utf-8", contentType.CharSet);

        AppUserDto? user = await response.Content.ReadFromJsonAsync<AppUserDto>();
        Assert.NotNull(user);
        Assert.Equal(expectedUser.UserName, user.UserName);
        Assert.Equal(expectedUser.Email, user.Email);
    }
}