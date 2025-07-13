using System.Text.Json;
using api.Constants;
using api.Dtos.StoryPart;
using api.IntegrationTests.Constants;
using api.IntegrationTests.Data;
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
    
    [Theory]
    [ClassData(typeof(AuthorsInStoryTestData))]
    public async Task JoinStorySession_WhenManyUsersJoinSession_ThenUsersAreAddedToSession(
        TestUserModel user1, 
        TestUserModel user2, 
        TestUserModel user3
        )
    {
        // Arrange
        const int storyId = AuthorsInStoryTestData.StoryId;
        using IStorySessionService storySessionService = _factory.Services.GetRequiredService<IStorySessionService>();
        
        await using HubConnection connection1 = _factory.CreateHubConnectionWithAuth(
            user1.UserName,
            user1.NameIdentifier,
            user1.Email,
            RoleConstants.User
        );
        
        await using HubConnection connection2 = _factory.CreateHubConnectionWithAuth(
            user2.UserName,
            user2.NameIdentifier,
            user2.Email,
            RoleConstants.User
        );
        
        await using HubConnection connection3 = _factory.CreateHubConnectionWithAuth(
            user3.UserName,
            user3.NameIdentifier,
            user3.Email,
            RoleConstants.User
        );

        // Act
        await connection1.StartAsync();
        await connection1.InvokeAsync("JoinStorySession", storyId);
        await connection2.StartAsync();
        await connection2.InvokeAsync("JoinStorySession", storyId);
        await connection3.StartAsync();
        await connection3.InvokeAsync("JoinStorySession", storyId);
    
        // Assert
        Assert.Equal(1, storySessionService.SessionsCount);
        
        var connections = storySessionService.GetSessionConnections(storyId.ToString());
        Assert.Equal(3, connections.Count);
        Assert.Contains(connection1.ConnectionId, connections);
        Assert.Contains(connection2.ConnectionId, connections);
        Assert.Contains(connection3.ConnectionId, connections);
    }

    [Fact]
    public async Task LeaveStorySession_WhenUserLeavesSession_ThenUserIsRemovedFromSession()
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
        await connection.InvokeAsync("LeaveStorySession", storyId);
        
        // Assert
        Assert.Equal(0, storySessionService.SessionsCount);
    }

    [Theory]
    [ClassData(typeof(AuthorsInStoryTestData))]
    public async Task SendStoryPart_WhenConnectedToStory_OtherUsersReceiveStoryPart(
        TestUserModel user1, 
        TestUserModel user2,
        TestUserModel user3
        )
    {
        // Arrange
        const int storyId = AuthorsInStoryTestData.StoryId;
        using IStorySessionService sessionService = _factory.Services.GetRequiredService<IStorySessionService>();
        await using HubConnection connection1 = _factory.CreateHubConnectionWithAuth(
            user1.UserName,
            user1.NameIdentifier,
            user1.Email,
            RoleConstants.User
        );
        await connection1.StartAsync();
        await connection1.InvokeAsync("JoinStorySession", storyId);
        
        await using HubConnection connection2 = _factory.CreateHubConnectionWithAuth(
            user2.UserName,
            user2.NameIdentifier,
            user2.Email,
            RoleConstants.User
        );
        await connection2.StartAsync();
        await connection2.InvokeAsync("JoinStorySession", storyId);
        
        await using HubConnection connection3 = _factory.CreateHubConnectionWithAuth(
            user3.UserName,
            user3.NameIdentifier,
            user3.Email,
            RoleConstants.User
        );
        await connection3.StartAsync();
        await connection3.InvokeAsync("JoinStorySession", storyId);
        
        // Act
        await connection1.InvokeAsync("SendStoryPart", storyId, "Test story part");
        
        // Assert
        connection2.On<int, StoryPartDto>("ReceiveStoryPart", (receivedStoryId, receivedStoryPart) =>
        {
            Assert.Equal(storyId, receivedStoryId);
            Assert.Equal("Test story part", receivedStoryPart.Text);
            Assert.Equal("neva_rosenbaum29", receivedStoryPart.UserName);
            Assert.Equal(storyId, receivedStoryPart.StoryId);
        });
        
        connection3.On<int, StoryPartDto>("ReceiveStoryPart", (receivedStoryId, receivedStoryPart) =>
        {
            Assert.Equal(storyId, receivedStoryId);
            Assert.Equal("Test story part", receivedStoryPart.Text);
            Assert.Equal("neva_rosenbaum29", receivedStoryPart.UserName);
            Assert.Equal(storyId, receivedStoryPart.StoryId);
        });
    }
}