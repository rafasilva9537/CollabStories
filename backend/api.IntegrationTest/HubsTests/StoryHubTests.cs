using System.Text.Json;
using api.Constants;
using api.Dtos.StoryPart;
using api.IntegrationTests.Constants;
using api.IntegrationTests.WebAppFactories;
using api.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace api.IntegrationTests.HubsTests;

[Collection(CollectionConstants.IntegrationTestsDatabase)]
// Remember to always dispose of IStorySessionService in every test
public class StoryHubTests : IClassFixture<AuthHandlerWebAppFactory>
{
    private readonly AuthHandlerWebAppFactory _factory;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public StoryHubTests(AuthHandlerWebAppFactory factory)
    {
        _factory = factory;
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };
    }


    [Fact]
    public async Task JoinStorySession_WhenStoryExists_JoinsSuccessfully()
    {
        // Arrange
        using IStorySessionService sessionService = _factory.Services.GetRequiredService<IStorySessionService>();
        await using HubConnection connection = _factory.CreateHubConnectionWithAuth(
            TestConstants.DefaultUserName,
            TestConstants.DefaultNameIdentifier,
            TestConstants.DefaultEmail,
            TestConstants.DefaultRole
        );
        const int storyId = 158;

        // Act/Assert
        await connection.StartAsync();
        await connection.InvokeAsync("JoinStorySession", storyId);
    }

    [Theory]
    [InlineData("JoinStory", 158)]
    [InlineData("JoinStorySession", "158")]
    [InlineData("JoinStorySession", null)]
    public async Task JoinStorySession_WithInvalidArguments_ThrowsException(string methodName, object storyId)
    {
        // Arrange
        using IStorySessionService sessionService = _factory.Services.GetRequiredService<IStorySessionService>();
        await using HubConnection connection = _factory.CreateHubConnectionWithAuth(
            "neva_rosenbaum29",
            "neva_rosenbaum29",
            "neva_rosenbaum2930@hotmail.com",
            RoleConstants.User
        );

        // Act
        await connection.StartAsync();
        
        // Assert
        Task InvokeHubMethod() => connection.InvokeAsync(methodName, storyId);
        await Assert.ThrowsAsync<HubException>(InvokeHubMethod);
    }
    
    [Fact]
    public async Task JoinStorySession_WhenNoUserJoinsStorySession_SessionIsNotCreated()
    {
        // Arrange
        using IStorySessionService sessionService = _factory.Services.GetRequiredService<IStorySessionService>();
        await using HubConnection connection = _factory.CreateHubConnectionWithAuth(
            TestConstants.DefaultUserName,
            TestConstants.DefaultNameIdentifier,
            TestConstants.DefaultEmail,
            TestConstants.DefaultRole
        );

        // Act
        await connection.StartAsync();
        
        // Assert
        Assert.Equal(0, sessionService.SessionsCount);
    }    
    
    [Fact]
    public async Task JoinStorySession_WhenUserJoinsSession_ThenUserIsAddedToSession()
    {
        // Arrange
        using IStorySessionService storySessionService = _factory.Services.GetRequiredService<IStorySessionService>();
        await using HubConnection connection = _factory.CreateHubConnectionWithAuth(
            TestConstants.DefaultUserName,
            TestConstants.DefaultNameIdentifier,
            TestConstants.DefaultEmail,
            TestConstants.DefaultRole
        );
        const int storyId = 158;

        // Act
        await connection.StartAsync();
        await connection.InvokeAsync("JoinStorySession", storyId);
    
        // Assert
        Assert.Equal(1, storySessionService.SessionsCount);
    
        var connections = storySessionService.GetSessionConnections(storyId.ToString());
        Assert.Equal(1, connections.Count);
        Assert.Contains(connection.ConnectionId, connections);
    }

    [Fact]
    public async Task SendStoryPart_WhenConnectedToStory_OtherUsersReceiveStoryPart()
    {
        // Arrange
        using IStorySessionService sessionService = _factory.Services.GetRequiredService<IStorySessionService>();
        await using HubConnection connection1 = _factory.CreateHubConnectionWithAuth(
            "neva_rosenbaum29",
            "neva_rosenbaum29",
            "neva_rosenbaum2930@hotmail.com",
            RoleConstants.User
        );
        await connection1.StartAsync();
        await connection1.InvokeAsync("JoinStorySession", 158);
        
        await using HubConnection connection2 = _factory.CreateHubConnectionWithAuth(
            "jovani.lueilwitz",
            "jovani.lueilwitz",
            "jovani.lueilwitz77@gmail.com",
            RoleConstants.User
        );
        await connection2.StartAsync();
        await connection2.InvokeAsync("JoinStorySession", 158);
        
        // Act
        await connection1.InvokeAsync("SendStoryPart", 158, "Test story part");
        
        // Assert
        connection2.On<int, StoryPartDto>("ReceiveStoryPart", (storyId, receivedStoryPart) =>
        {
            Assert.Equal(158, storyId);
            Assert.Equal("Test story part", receivedStoryPart.Text);
            Assert.Equal("neva_rosenbaum29", receivedStoryPart.UserName);
        });
    }
}