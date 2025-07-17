using System.Text.Json;
using api.Constants;
using api.Dtos.StoryPart;
using api.IntegrationTests.Constants;
using api.IntegrationTests.Data;
using api.IntegrationTests.WebAppFactories;
using api.Interfaces;
using api.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;

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
            TestConstants.DefaultUserName,
            TestConstants.DefaultNameIdentifier,
            TestConstants.DefaultEmail,
            TestConstants.DefaultRole
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
    public async Task JoinStorySession_WhenAUserJoinsSession_TimerInfoReturnsCorrectValues()
    {
        // Arrange
        DateTimeOffset fakeDateNow = DateTimeOffset.Parse("2024-12-05T05:09:15.8676393+00:00");;
        IDateTimeProvider? dateTimeProvider = Substitute.For<IDateTimeProvider>();
        dateTimeProvider.UtcNow.Returns(fakeDateNow);

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(IDateTimeProvider));
                services.AddTransient<IDateTimeProvider>(_ => dateTimeProvider);
            });
        });
        using IStorySessionService storySessionService = factory.Services.GetRequiredService<IStorySessionService>();
        await using HubConnection connection = factory.CreateHubConnectionWithAuth(
            TestConstants.DefaultUserName,
            TestConstants.DefaultNameIdentifier,
            TestConstants.DefaultEmail,
            TestConstants.DefaultRole
        );
        const int storyId = TestConstants.DefaultJoinedStoryId;

        // Act
        await connection.StartAsync();
        await connection.InvokeAsync("JoinStorySession", storyId);

        // Assert
        double actualTimerSeconds = storySessionService.GetSessionTimerSeconds(storyId.ToString());
        int actualTurnDurationSeconds = storySessionService.GetSessionTurnDurationSeconds(storyId.ToString());
        Assert.Equal(5, actualTimerSeconds);
        Assert.Equal(1447, actualTurnDurationSeconds);
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
        const int storyId = TestConstants.DefaultJoinedStoryId;

        // Act
        await connection.StartAsync();
        await connection.InvokeAsync("JoinStorySession", storyId);
        await connection.InvokeAsync("LeaveStorySession", storyId);
        
        // Assert
        Assert.Equal(0, storySessionService.SessionsCount);
    }
    
    [Theory]
    [ClassData(typeof(AuthorsInStoryTestData))]
    public async Task LeaveStorySession_WhenManyUsersLeaveSession_ThenUsersAreRemovedFromSession(
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
        await connection1.StartAsync();
        await connection1.InvokeAsync("JoinStorySession", storyId);
        await connection2.StartAsync();
        await connection2.InvokeAsync("JoinStorySession", storyId);
        await connection3.StartAsync();
        await connection3.InvokeAsync("JoinStorySession", storyId);

        // Act
        await connection1.InvokeAsync("LeaveStorySession", storyId);
        await connection2.InvokeAsync("LeaveStorySession", storyId);
        await connection3.InvokeAsync("LeaveStorySession", storyId);
        
        // Assert
        Assert.False(storySessionService.SessionIsActive(storyId.ToString()));
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
        const string storyPartTextFromUser1 = "Test story part";
        
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
        TaskCompletionSource connection2ReceivedStoryPartTcs = new();
        TaskCompletionSource connection3ReceivedStoryPartTcs = new();
        
        int? storyIdConnection2 = null;
        StoryPartDto? storyPartConnection2 = null;
        connection2.On<int, StoryPartDto>("ReceiveStoryPart", (receivedStoryId, receivedStoryPart) =>
        {
            storyIdConnection2 = receivedStoryId;
            storyPartConnection2 = receivedStoryPart;
            connection2ReceivedStoryPartTcs.SetResult();
        });
        
        int? storyIdConnection3 = null;
        StoryPartDto? storyPartConnection3 = null;
        connection3.On<int, StoryPartDto>("ReceiveStoryPart", (receivedStoryId, receivedStoryPart) =>
        {
            storyIdConnection3 = receivedStoryId;
            storyPartConnection3 = receivedStoryPart;
            connection3ReceivedStoryPartTcs.SetResult();
        });
        
        await connection1.InvokeAsync("SendStoryPart", storyId, storyPartTextFromUser1);
        
        Task timeout = Task.Delay(TimeSpan.FromSeconds(5));
        Task allConnectionsReceivedStoryPart = Task.WhenAll(connection2ReceivedStoryPartTcs.Task, connection3ReceivedStoryPartTcs.Task);
        await Task.WhenAny(timeout, allConnectionsReceivedStoryPart);
        
        
        // Assert
        Assert.False(timeout.IsCompleted, "Timeout reached");
        
        Assert.NotNull(storyPartConnection2);
        Assert.NotNull(storyIdConnection2);
        Assert.Equal(storyId, storyIdConnection2);
        Assert.Equal(storyPartTextFromUser1, storyPartConnection2.Text);
        Assert.Equal(user1.UserName, storyPartConnection2.UserName);
        Assert.Equal(storyId, storyPartConnection2.StoryId);
        
        Assert.NotNull(storyPartConnection3);
        Assert.NotNull(storyIdConnection3);
        Assert.Equal(storyId, storyIdConnection3);
        Assert.Equal(storyPartTextFromUser1, storyPartConnection3.Text);
        Assert.Equal(user1.UserName, storyPartConnection3.UserName);
        Assert.Equal(storyId, storyPartConnection3.StoryId);
    }
}