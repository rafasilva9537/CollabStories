using api.Constants;
using api.Dtos.StoryPart;
using api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace api.Hubs;

public interface IStoryClient
{
    Task ReceiveStoryPart(int storyId, StoryPartDto createdStoryPart);
    Task MessageFailed(string message);
    Task UserDisconnected(string userName);
    Task UserConnected(string userName);
    Task SetInitialState(string? currentAuthorUsername, DateTimeOffset turnEndTime);
    Task ReceiveTurnChange(string newAuthorUsername, DateTimeOffset turnEndTime);
}

[Authorize]
public class StoryHub : Hub<IStoryClient>
{
    private readonly IStoryService _storyService;
    private readonly ILogger<StoryHub> _logger;
    private readonly IStorySessionService _storySessionService;
    public StoryHub(IStoryService storyService, ILogger<StoryHub> logger, IStorySessionService storySessionService)
    {
        _storyService = storyService;
        _logger = logger;
        _storySessionService = storySessionService;
    }

    public async Task JoinStorySession(int storyId)
    {
        string? userName = Context.User?.Identity?.Name;
        if (userName is null)
        {
            _logger.LogWarning("Unauthenticated user tried to join story {StoryId}.", storyId);
            return;
        }
        
        bool storyExists = await _storyService.StoryExistsAsync(storyId);
        if (!storyExists)
        {
            _logger.LogWarning("User '{UserName}' tried to join story {StoryId} that doesn't exist.", userName, storyId);
            return;       
        }
        
        bool isStoryAuthor = await _storyService.IsStoryAuthorAsync(userName, storyId);
        if (!isStoryAuthor)
        {
            _logger.LogWarning("User '{UserName}' tried to join story {StoryId} but they are not an author of it.", userName, storyId);
            return;
        }
        
        await Groups.AddToGroupAsync(Context.ConnectionId, storyId.ToString());
        string storySessionId = storyId.ToString();
        if (!_storySessionService.SessionIsActive(storySessionId))
        {
            await _storySessionService.AddSessionAsync(storySessionId, _storyService);
        }
        _storySessionService.AddConnectionToSession(storySessionId, Context.ConnectionId);

        string? currentAuthorUsername = await _storyService.GetCurrentAuthorUserNameAsync(storyId);
        DateTimeOffset turnEndTime = _storySessionService.GetTurnEndTime(storySessionId);
        await Clients.Caller.SetInitialState(currentAuthorUsername, turnEndTime);
        
        await Clients.Group(storySessionId).UserConnected(userName);
    }

    public async Task LeaveStorySession(int storyId)
    {
        string? userName = Context.User?.Identity?.Name;
        if (userName is null)
        {
            _logger.LogWarning("Unauthenticated user tried to leave story {StoryId}.", storyId);
            return;       
        }
        
        bool storyExists = await _storyService.StoryExistsAsync(storyId);
        if (!storyExists)
        {
            _logger.LogWarning("User '{UserName}' tried to leave story {StoryId} that doesn't exist.", userName, storyId);
            return;       
        } 
        
        bool isStoryAuthor = await _storyService.IsStoryAuthorAsync(userName, storyId);
        if (!isStoryAuthor)
        {
            _logger.LogWarning("User '{UserName}' tried to leave story {StoryId} but they are not an author of it.", userName, storyId);
            return;
        }
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, storyId.ToString());
        string storySessionId = storyId.ToString();
        if (_storySessionService.SessionIsActive(storySessionId))
        {
            _storySessionService.RemoveConnectionFromSession(storySessionId, Context.ConnectionId);
            if (_storySessionService.SessionIsEmpty(storySessionId))
            {
                _storySessionService.RemoveSession(storySessionId);
            }
        }
        
        await Clients.Group(storyId.ToString()).UserDisconnected(userName);
    }
    
    public async Task SendStoryPart(int storyId, string storyPartText)
    {
        if (string.IsNullOrWhiteSpace(storyPartText))
        {
            await Clients.Caller.MessageFailed("Unable to send message. The story part text is empty.");
            _logger.LogInformation("User attempted to send empty story part to story {StoryId}.", storyId);
            return;
        }

        CreateStoryPartDto newStoryPart = new()
        {
            Text = storyPartText
        };

        string? userName = Context.User?.Identity?.Name;

        if(userName is null)
        {
            await Clients.Caller.MessageFailed("Unable to send message. It wasn't possible to get the logged-in user.");
            _logger.LogWarning("Unauthenticated user tried to send message to story {StoryId}.", storyId);
            return;
        }

        StoryPartDto? createdStoryPart = await _storyService.CreateStoryPartAsync(storyId, userName, newStoryPart);
        if(createdStoryPart is null)
        {
            await Clients.Caller.MessageFailed("Unable to send message. User isn't the current author of the story.");
            _logger.LogWarning("User '{UserName}' tried to send message to story {StoryId} but they are not the current author.", userName, storyId);
            return;
        }
        
        await Clients.Group(storyId.ToString()).ReceiveStoryPart(storyId, createdStoryPart);
    }
    
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Connection established. ConnectionId: {ConnectionId}, User: {UserName}.", Context.ConnectionId, Context.User?.Identity?.Name);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Connection disconnected. ConnectionId: {ConnectionId}, User: {UserName}, Error: {Error}.", Context.ConnectionId, Context.User?.Identity?.Name, exception?.Message);
        await base.OnDisconnectedAsync(exception);
    }
}