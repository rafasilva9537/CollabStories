using System.Text.Json;
using api.Constants;
using Microsoft.AspNetCore.SignalR.Client;
using Xunit.Abstractions;

namespace api.IntegrationTests.Hubs;

[Collection(CollectionConstants.IntegrationTestsDatabase)]
public class StoryHubTests : IClassFixture<CustomWebAppFactory>
{
    private readonly CustomWebAppFactory _factory;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly ITestOutputHelper _testOutputHelper;

    public StoryHubTests(CustomWebAppFactory factory, ITestOutputHelper testOutputHelper)
    {
        _factory = factory;
        _testOutputHelper = testOutputHelper;
        
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };
    }

    [Fact]
    public async Task JoinStorySession_WhenStoryExists_ReturnsSuccess()
    {
        HubConnection connection = _factory.CreateHubConnectionWithAuth(
            "neva_rosenbaum29", 
            "neva_rosenbaum29", 
            "neva_rosenbaum2930@hotmail.com", 
            RoleConstants.User
        );

        await connection.StartAsync();
    }
}