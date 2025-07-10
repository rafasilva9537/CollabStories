using System.Text.Json;
using api.Constants;
using api.Dtos.StoryPart;
using Microsoft.AspNetCore.SignalR;
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
    public async Task JoinStorySession_WhenStoryExists_JoinsSuccessfully()
    {
        // Arrange
        await using HubConnection connection = _factory.CreateHubConnectionWithAuth(
            "neva_rosenbaum29",
            "neva_rosenbaum29",
            "neva_rosenbaum2930@hotmail.com",
            RoleConstants.User
        );

        // Act/Assert
        await connection.StartAsync();
        await connection.InvokeAsync("JoinStorySession", "neva_rosenbaum29", 158);
    }

    [Theory]
    [InlineData("JoinStory", 158)]
    [InlineData("JoinStorySession", "158")]
    [InlineData("JoinStorySession", null)]
    public async Task JoinStorySession_WithInvalidArguments_ThrowsException(string methodName, object storyId)
    {
        // Arrange
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
    public async Task SendStoryPart_WhenConnectedToStory_OtherUsersReceiveStoryPart()
    {
        // Arrange
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