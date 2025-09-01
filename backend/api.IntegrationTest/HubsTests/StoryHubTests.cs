// using System.Text.Json;
// using api.Constants;
// using api.Dtos.StoryPart;
// using api.IntegrationTests.Constants;
// using api.IntegrationTests.Data;
// using api.IntegrationTests.WebAppFactories;
// using api.Interfaces;
// using api.Models;
// using Microsoft.AspNetCore.SignalR;
// using Microsoft.AspNetCore.SignalR.Client;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.DependencyInjection.Extensions;
// using NSubstitute;
//
// namespace api.IntegrationTests.HubsTests;
//
// [Collection(CollectionConstants.IntegrationTestsDatabase)]
// // Remember to always dispose of IStorySessionService in every test
// public class StoryHubTests : IClassFixture<AuthHandlerWebAppFactory>
// {
//     private readonly AuthHandlerWebAppFactory _factory;
//     private readonly JsonSerializerOptions _jsonSerializerOptions;
//
//     public StoryHubTests(AuthHandlerWebAppFactory factory)
//     {
//         _factory = factory;
//         _jsonSerializerOptions = new JsonSerializerOptions
//         {
//             PropertyNameCaseInsensitive = true,
//         };
//     }
//
//
//     [Fact]
//     public async Task JoinStorySession_WhenStoryExists_JoinsSuccessfully()
//     {
//         // Arrange
//         using IStorySessionService sessionService = _factory.Services.GetRequiredService<IStorySessionService>();
//         await using HubConnection connection = _factory.CreateHubConnectionWithAuth(
//             TestConstants.DefaultUserName,
//             TestConstants.DefaultNameIdentifier,
//             TestConstants.DefaultEmail,
//             TestConstants.DefaultRole
//         );
//         const int storyId = 158;
//
//         // Act/Assert
//         await connection.StartAsync();
//         await connection.InvokeAsync("JoinStorySession", storyId);
//     }
//
//     [Theory]
//     [InlineData("JoinStory", 158)]
//     [InlineData("JoinStorySession", "158")]
//     [InlineData("JoinStorySession", null)]
//     public async Task JoinStorySession_WithInvalidArguments_ThrowsException(string methodName, object storyId)
//     {
//         // Arrange
//         using IStorySessionService sessionService = _factory.Services.GetRequiredService<IStorySessionService>();
//         await using HubConnection connection = _factory.CreateHubConnectionWithAuth(
//             TestConstants.DefaultUserName,
//             TestConstants.DefaultNameIdentifier,
//             TestConstants.DefaultEmail,
//             TestConstants.DefaultRole
//         );
//
//         // Act
//         await connection.StartAsync();
//         
//         // Assert
//         Task InvokeHubMethod() => connection.InvokeAsync(methodName, storyId);
//         await Assert.ThrowsAsync<HubException>(InvokeHubMethod);
//     }
//     
//     [Fact]
//     public async Task JoinStorySession_WhenNoUserJoinsStorySession_SessionIsNotCreated()
//     {
//         // Arrange
//         using IStorySessionService sessionService = _factory.Services.GetRequiredService<IStorySessionService>();
//         await using HubConnection connection = _factory.CreateHubConnectionWithAuth(
//             TestConstants.DefaultUserName,
//             TestConstants.DefaultNameIdentifier,
//             TestConstants.DefaultEmail,
//             TestConstants.DefaultRole
//         );
//
//         // Act
//         await connection.StartAsync();
//         
//         // Assert
//         Assert.Equal(0, sessionService.SessionsCount);
//     }    
//     
//     [Fact]
//     public async Task JoinStorySession_WhenUserJoinsSession_ThenUserIsAddedToSession()
//     {
//         // Arrange
//         using IStorySessionService storySessionService = _factory.Services.GetRequiredService<IStorySessionService>();
//         await using HubConnection connection = _factory.CreateHubConnectionWithAuth(
//             TestConstants.DefaultUserName,
//             TestConstants.DefaultNameIdentifier,
//             TestConstants.DefaultEmail,
//             TestConstants.DefaultRole
//         );
//         const int storyId = 158;
//
//         // Act
//         await connection.StartAsync();
//         await connection.InvokeAsync("JoinStorySession", storyId);
//     
//         // Assert
//         Assert.Equal(1, storySessionService.SessionsCount);
//     
//         var connections = storySessionService.GetSessionConnections(storyId.ToString());
//         Assert.Equal(1, connections.Count);
//         Assert.Contains(connection.ConnectionId, connections);
//     }
//     
//     [Theory]
//     [ClassData(typeof(StoryAndAuthorsTestData))]
//     public async Task JoinStorySession_WhenManyUsersJoinSession_ThenUsersAreAddedToSession(
//         TestUserModel user1, 
//         TestUserModel user2, 
//         TestUserModel user3,
//         Story story)
//     {
//         // Arrange
//         using IStorySessionService storySessionService = _factory.Services.GetRequiredService<IStorySessionService>();
//         
//         await using HubConnection connection1 = _factory.CreateHubConnectionWithAuth(
//             user1.UserName,
//             user1.NameIdentifier,
//             user1.Email,
//             RoleConstants.User
//         );
//         
//         await using HubConnection connection2 = _factory.CreateHubConnectionWithAuth(
//             user2.UserName,
//             user2.NameIdentifier,
//             user2.Email,
//             RoleConstants.User
//         );
//         
//         await using HubConnection connection3 = _factory.CreateHubConnectionWithAuth(
//             user3.UserName,
//             user3.NameIdentifier,
//             user3.Email,
//             RoleConstants.User
//         );
//
//         // Act
//         await connection1.StartAsync();
//         await connection1.InvokeAsync("JoinStorySession", story.Id);
//         await connection2.StartAsync();
//         await connection2.InvokeAsync("JoinStorySession", story.Id);
//         await connection3.StartAsync();
//         await connection3.InvokeAsync("JoinStorySession", story.Id);
//     
//         // Assert
//         Assert.Equal(1, storySessionService.SessionsCount);
//         
//         var connections = storySessionService.GetSessionConnections(story.Id.ToString());
//         Assert.Equal(3, connections.Count);
//         Assert.Contains(connection1.ConnectionId, connections);
//         Assert.Contains(connection2.ConnectionId, connections);
//         Assert.Contains(connection3.ConnectionId, connections);
//     }
//
//     [Fact]
//     public async Task JoinStorySession_WhenAUserJoinsSession_TimerInfoReturnsCorrectValues()
//     {
//         // Arrange
//         DateTimeOffset fakeDateNow = StoryAndAuthorsTestData.AuthorsMembershipChangeDate + TimeSpan.FromSeconds(5);
//         IDateTimeProvider? dateTimeProvider = Substitute.For<IDateTimeProvider>();
//         dateTimeProvider.UtcNow.Returns(fakeDateNow);
//
//         var factory = _factory.WithWebHostBuilder(builder =>
//         {
//             builder.ConfigureServices(services =>
//             {
//                 services.RemoveAll(typeof(IDateTimeProvider));
//                 services.AddTransient<IDateTimeProvider>(_ => dateTimeProvider);
//             });
//         });
//         using IStorySessionService storySessionService = factory.Services.GetRequiredService<IStorySessionService>();
//         await using HubConnection connection = factory.CreateHubConnectionWithAuth(
//             TestConstants.DefaultUserName,
//             TestConstants.DefaultNameIdentifier,
//             TestConstants.DefaultEmail,
//             TestConstants.DefaultRole
//         );
//         const int storyId = TestConstants.DefaultJoinedStoryId;
//
//         // Act
//         await connection.StartAsync();
//         await connection.InvokeAsync("JoinStorySession", storyId);
//
//         // Assert
//         double actualTimerSeconds = storySessionService.GetSessionTimerSeconds(storyId.ToString());
//         int actualTurnDurationSeconds = storySessionService.GetSessionTurnDurationSeconds(storyId.ToString());
//         Assert.Equal(5, actualTimerSeconds);
//         Assert.Equal(1447, actualTurnDurationSeconds);
//     }
//
//     [Fact]
//     public async Task LeaveStorySession_WhenUserLeavesSession_ThenUserIsRemovedFromSession()
//     {
//         // Arrange
//         using IStorySessionService storySessionService = _factory.Services.GetRequiredService<IStorySessionService>();
//         await using HubConnection connection = _factory.CreateHubConnectionWithAuth(
//             TestConstants.DefaultUserName,
//             TestConstants.DefaultNameIdentifier,
//             TestConstants.DefaultEmail,
//             TestConstants.DefaultRole
//         );
//         const int storyId = TestConstants.DefaultJoinedStoryId;
//
//         // Act
//         await connection.StartAsync();
//         await connection.InvokeAsync("JoinStorySession", storyId);
//         await connection.InvokeAsync("LeaveStorySession", storyId);
//         
//         // Assert
//         Assert.Equal(0, storySessionService.SessionsCount);
//     }
//     
//     [Theory]
//     [ClassData(typeof(StoryAndAuthorsTestData))]
//     public async Task LeaveStorySession_WhenManyUsersLeaveSession_ThenUsersAreRemovedFromSession(
//         TestUserModel user1,
//         TestUserModel user2,
//         TestUserModel user3,
//         Story story)
//     {
//         // Arrange
//         using IStorySessionService storySessionService = _factory.Services.GetRequiredService<IStorySessionService>();
//         
//         await using HubConnection connection1 = _factory.CreateHubConnectionWithAuth(
//             user1.UserName,
//             user1.NameIdentifier,
//             user1.Email,
//             RoleConstants.User
//         );
//         
//         await using HubConnection connection2 = _factory.CreateHubConnectionWithAuth(
//             user2.UserName,
//             user2.NameIdentifier,
//             user2.Email,
//             RoleConstants.User
//         );
//         
//         await using HubConnection connection3 = _factory.CreateHubConnectionWithAuth(
//             user3.UserName,
//             user3.NameIdentifier,
//             user3.Email,
//             RoleConstants.User
//         );
//         await connection1.StartAsync();
//         await connection1.InvokeAsync("JoinStorySession", story.Id);
//         await connection2.StartAsync();
//         await connection2.InvokeAsync("JoinStorySession", story.Id);
//         await connection3.StartAsync();
//         await connection3.InvokeAsync("JoinStorySession", story.Id);
//
//         // Act
//         await connection1.InvokeAsync("LeaveStorySession", story.Id);
//         await connection2.InvokeAsync("LeaveStorySession", story.Id);
//         await connection3.InvokeAsync("LeaveStorySession", story.Id);
//         
//         // Assert
//         Assert.False(storySessionService.SessionIsActive(story.Id.ToString()));
//         Assert.Equal(0, storySessionService.SessionsCount);
//     }
//
//     [Theory]
//     [ClassData(typeof(StoryAndAuthorsTestData))]
//     public async Task SendStoryPart_WhenConnectedToStory_OtherUsersReceiveStoryPart(
//         TestUserModel user1, 
//         TestUserModel user2,
//         TestUserModel user3,
//         Story story)
//     {
//         // Arrange
//         using IStorySessionService sessionService = _factory.Services.GetRequiredService<IStorySessionService>();
//         
//         await using HubConnection connection1 = _factory.CreateHubConnectionWithAuth(
//             user1.UserName,
//             user1.NameIdentifier,
//             user1.Email,
//             RoleConstants.User
//         );
//         await connection1.StartAsync();
//         await connection1.InvokeAsync("JoinStorySession", story.Id);
//         const string storyPartTextFromUser1 = "Test story part";
//         
//         await using HubConnection connection2 = _factory.CreateHubConnectionWithAuth(
//             user2.UserName,
//             user2.NameIdentifier,
//             user2.Email,
//             RoleConstants.User
//         );
//         await connection2.StartAsync();
//         await connection2.InvokeAsync("JoinStorySession", story.Id);
//         
//         await using HubConnection connection3 = _factory.CreateHubConnectionWithAuth(
//             user3.UserName,
//             user3.NameIdentifier,
//             user3.Email,
//             RoleConstants.User
//         );
//         await connection3.StartAsync();
//         await connection3.InvokeAsync("JoinStorySession", story.Id);
//         
//         
//         // Act
//         TaskCompletionSource connection2ReceivedStoryPartTcs = new();
//         TaskCompletionSource connection3ReceivedStoryPartTcs = new();
//         
//         int? storyIdConnection2 = null;
//         StoryPartDto? storyPartConnection2 = null;
//         connection2.On<int, StoryPartDto>("ReceiveStoryPart", (receivedStoryId, receivedStoryPart) =>
//         {
//             storyIdConnection2 = receivedStoryId;
//             storyPartConnection2 = receivedStoryPart;
//             connection2ReceivedStoryPartTcs.SetResult();
//         });
//         
//         int? storyIdConnection3 = null;
//         StoryPartDto? storyPartConnection3 = null;
//         connection3.On<int, StoryPartDto>("ReceiveStoryPart", (receivedStoryId, receivedStoryPart) =>
//         {
//             storyIdConnection3 = receivedStoryId;
//             storyPartConnection3 = receivedStoryPart;
//             connection3ReceivedStoryPartTcs.SetResult();
//         });
//         
//         await connection1.InvokeAsync("SendStoryPart", story.Id, storyPartTextFromUser1);
//         
//         Task timeout = Task.Delay(TimeSpan.FromSeconds(5));
//         Task allConnectionsReceivedStoryPart = Task.WhenAll(connection2ReceivedStoryPartTcs.Task, connection3ReceivedStoryPartTcs.Task);
//         await Task.WhenAny(timeout, allConnectionsReceivedStoryPart);
//         
//         
//         // Assert
//         Assert.False(timeout.IsCompleted, "Timeout reached");
//         
//         Assert.NotNull(storyPartConnection2);
//         Assert.NotNull(storyIdConnection2);
//         Assert.Equal(story.Id, storyIdConnection2);
//         Assert.Equal(storyPartTextFromUser1, storyPartConnection2.Text);
//         Assert.Equal(user1.UserName, storyPartConnection2.UserName);
//         Assert.Equal(story.Id, storyPartConnection2.StoryId);
//         
//         Assert.NotNull(storyPartConnection3);
//         Assert.NotNull(storyIdConnection3);
//         Assert.Equal(story.Id, storyIdConnection3);
//         Assert.Equal(storyPartTextFromUser1, storyPartConnection3.Text);
//         Assert.Equal(user1.UserName, storyPartConnection3.UserName);
//         Assert.Equal(story.Id, storyPartConnection3.StoryId);
//     }
//
//     [Theory]
//     [ClassData(typeof(StoryAndAuthorsTestData))]
//     public async Task ReceiveTimerSeconds_WithUsersInSession_ReturnsTimerSeconds(
//         TestUserModel user1,
//         TestUserModel user2,
//         TestUserModel user3,
//         Story story)
//     {
//         // Arrange
//         const double expectedSeconds = 20;
//         double secondsPassedFromMembersChange = (story.TurnDurationSeconds * 2) + 20;
//         
//         DateTimeOffset fakeDateNow = StoryAndAuthorsTestData.AuthorsMembershipChangeDate + TimeSpan.FromSeconds(secondsPassedFromMembersChange);
//         IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
//         dateTimeProvider.UtcNow.Returns(fakeDateNow);
//         
//         var customFactory = _factory.WithWebHostBuilder(builder =>
//         {
//             builder.ConfigureServices(services =>
//             {
//                 services.RemoveAll(typeof(IDateTimeProvider));
//                 services.AddTransient<IDateTimeProvider>(_ => dateTimeProvider);
//             });
//         });
//         
//         await using HubConnection connection1 = customFactory.CreateHubConnectionWithAuth(
//             user1.UserName,
//             user1.NameIdentifier,
//             user1.Email,
//             RoleConstants.User
//         );
//         await using HubConnection connection2 = customFactory.CreateHubConnectionWithAuth(
//             user2.UserName,
//             user2.NameIdentifier,
//             user2.Email,
//             RoleConstants.User
//         );        
//         await using HubConnection connection3 = customFactory.CreateHubConnectionWithAuth(
//             user3.UserName,
//             user3.NameIdentifier,
//             user3.Email,
//             RoleConstants.User
//         );
//         await connection1.StartAsync();
//         await connection2.StartAsync();
//         await connection3.StartAsync();
//         
//         
//         // Act
//         await connection1.InvokeAsync("JoinStorySession", StoryAndAuthorsTestData.StoryId);
//         await connection2.InvokeAsync("JoinStorySession", StoryAndAuthorsTestData.StoryId);
//         await connection3.InvokeAsync("JoinStorySession", StoryAndAuthorsTestData.StoryId);
//         
//         TaskCompletionSource connection1TimerSecondsTcs = new();
//         TaskCompletionSource connection2TimerSecondsTcs = new();
//         TaskCompletionSource connection3TimerSecondsTcs = new();
//         
//         double? connection1TimerSeconds = null;
//         connection1.On<double>("ReceiveTimerSeconds", (timerSeconds) =>
//         {
//             connection1TimerSeconds = timerSeconds;
//             if(!connection1TimerSecondsTcs.Task.IsCompleted) connection1TimerSecondsTcs.SetResult();
//         });
//         
//         double? connection2TimerSeconds = null;
//         connection2.On<double>("ReceiveTimerSeconds", (timerSeconds) =>
//         {
//             connection2TimerSeconds = timerSeconds;
//             if(!connection2TimerSecondsTcs.Task.IsCompleted) connection2TimerSecondsTcs.SetResult();
//         });
//         
//         double? connection3TimerSeconds = null;
//         connection3.On<double>("ReceiveTimerSeconds", (timerSeconds) =>
//         {
//             connection3TimerSeconds = timerSeconds;
//             if(!connection3TimerSecondsTcs.Task.IsCompleted) connection3TimerSecondsTcs.SetResult();
//         });
//         
//         Task timeout = Task.Delay(TimeSpan.FromSeconds(10));
//         Task allConnectionsReceivedTimerSeconds = Task.WhenAll(
//             connection1TimerSecondsTcs.Task, 
//             connection2TimerSecondsTcs.Task, 
//             connection3TimerSecondsTcs.Task
//         );
//         await Task.WhenAny(timeout, allConnectionsReceivedTimerSeconds);
//         
//         
//         // Assert
//         Assert.False(timeout.IsCompleted, "Timeout reached");
//         
//         Assert.NotNull(connection1TimerSeconds);
//         Assert.NotNull(connection2TimerSeconds);
//         Assert.NotNull(connection3TimerSeconds);
//         
//         Assert.Equal(expectedSeconds, connection1TimerSeconds);
//         Assert.Equal(expectedSeconds, connection2TimerSeconds);
//         Assert.Equal(expectedSeconds, connection3TimerSeconds);
//
//         await Task.Delay(TimeSpan.FromSeconds(2));
//         
//         Assert.Equal(expectedSeconds-2, connection1TimerSeconds);
//         Assert.Equal(expectedSeconds-2, connection2TimerSeconds);
//         Assert.Equal(expectedSeconds-2, connection3TimerSeconds);
//         
//         await Task.Delay(TimeSpan.FromSeconds(1));
//         
//         Assert.Equal(expectedSeconds-3, connection1TimerSeconds);
//         Assert.Equal(expectedSeconds-3, connection2TimerSeconds);
//         Assert.Equal(expectedSeconds-3, connection3TimerSeconds);
//     }
// }