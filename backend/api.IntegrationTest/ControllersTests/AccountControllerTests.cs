using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using api.Dtos.AppUser;
using api.IntegrationTests.Constants;
using api.IntegrationTests.WebAppFactories;

namespace api.IntegrationTests.ControllersTests;

[Collection(CollectionConstants.IntegrationTestsDatabase)]
public class AccountControllerTests : IClassFixture<AuthHandlerWebAppFactory>
{
    private readonly AuthHandlerWebAppFactory _factory;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    
    public AccountControllerTests(AuthHandlerWebAppFactory factory)
    {
        _factory = factory;
        _jsonSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
        };
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
        
        string? jsonResponse = await response.Content.ReadAsStringAsync();
        Assert.NotNull(jsonResponse);
        var users = JsonSerializer.Deserialize<IList<UserMainInfoDto>>(jsonResponse, _jsonSerializerOptions);
        
        Assert.NotNull(users);
        Assert.Equal(expectedUsersCount, users.Count);
        Assert.Equal(containsUserWithId1, users.Any(u => u.Id == 1));
    }
    
    [Fact]
    public async Task GetUser_WhenAuthenticated_ReturnsExpectedUser()
    {
        // Arrange
        HttpClient client = _factory.CreateClientWithAuth(
            TestConstants.DefaultUserName, 
            TestConstants.DefaultNameIdentifier, 
            TestConstants.DefaultEmail, 
            TestConstants.DefaultRole
        );
        
        // Act
        HttpResponseMessage response = await client.GetAsync($"/accounts/{TestConstants.DefaultUserName}");
        
        // Assert
        MediaTypeHeaderValue? contentType = response.Content.Headers.ContentType;
        
        response.EnsureSuccessStatusCode();
        Assert.NotNull(contentType);
        Assert.Equal("utf-8", contentType.CharSet);
        Assert.Equal("application/json", contentType.MediaType);
        
        string jsonResponse = await response.Content.ReadAsStringAsync();
        AppUserDto? user = JsonSerializer.Deserialize<AppUserDto>(jsonResponse, _jsonSerializerOptions);
        Assert.NotNull(user);
        Assert.Equal(TestConstants.DefaultUserName, user.UserName);
        Assert.Equal(TestConstants.DefaultEmail, user.Email);
    }
}