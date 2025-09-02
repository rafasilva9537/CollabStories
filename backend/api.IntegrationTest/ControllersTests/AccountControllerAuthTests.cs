using System.Net;
using System.Text.Json;
using api.IntegrationTests.Constants;
using api.IntegrationTests.WebAppFactories;

namespace api.IntegrationTests.ControllersTests;

/// <summary>
/// Contains integration tests for authentication-related functionalities of the AccountController.
/// These tests ensure that the AccountController behaves correctly when authentication is required.
/// </summary>
[Collection(CollectionConstants.IntegrationTestsDatabase)]
public class AccountControllerAuthTests : IClassFixture<CustomWebAppFactory>
{
    private readonly CustomWebAppFactory _factory;
    
    public AccountControllerAuthTests(CustomWebAppFactory factory)
    {
        _factory = factory;
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