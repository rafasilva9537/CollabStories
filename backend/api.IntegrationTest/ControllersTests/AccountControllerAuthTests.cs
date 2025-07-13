using System.Net;
using System.Text.Json;
using api.IntegrationTests.Constants;
using api.IntegrationTests.WebAppFactories;

namespace api.IntegrationTests.ControllersTests;

[Collection(CollectionConstants.IntegrationTestsDatabase)]
public class AccountControllerAuthTests : IClassFixture<CustomWebAppFactory>
{
    private readonly CustomWebAppFactory _factory;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    
    public AccountControllerAuthTests(CustomWebAppFactory factory)
    {
        _factory = factory;
        _jsonSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
        };
    }
    
    [Fact]
    public async Task GetUser_WhenNotAuthenticated_ReturnsUnauthorized()
    {
        // Arrange
        HttpClient client = _factory.CreateClient();
        
        // Act
        HttpResponseMessage response = await client.GetAsync($"/accounts/{TestConstants.DefaultUserName}");
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);;
    }
}