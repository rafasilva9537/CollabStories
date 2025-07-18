using api.Dtos.StoryPart;
using api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace api.Hubs;

public interface IStoryClient
{
    Task ReceiveStoryPart(int storyId, StoryPartDto createdStoryPart);
    Task ReceiveTimerSeconds(int seconds);
    Task MessageFailed(string message);
    Task UserDisconnected(string userName);
    Task UserConnected(string userName);
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
            _logger.LogWarning("Non logged user tried to join story {StoryId}.", userName);
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
            _logger.LogWarning("User '{UserName}' tried to join story {StoryId} that he is not a author of.", userName, storyId);
            return;
        }
        
        await Groups.AddToGroupAsync(Context.ConnectionId, storyId.ToString());
        string storySessionId = storyId.ToString();
        if (!_storySessionService.SessionIsActive(storySessionId))
        {
            await _storySessionService.AddSessionAsync(storySessionId, _storyService);
            // TODO: add timer
        }
        _storySessionService.AddConnectionToSession(storySessionId, Context.ConnectionId);
        
        await Clients.Group(storySessionId).UserConnected(userName);
    }

    public async Task LeaveStorySession(int storyId)
    {
        string? userName = Context.User?.Identity?.Name;
        if (userName is null)
        {
            _logger.LogWarning("Non logged user tried to leave story {StoryId}.", userName);
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
            _logger.LogWarning("User '{UserName}' tried to join story {StoryId} that he is not a author of.", userName, storyId);
            return;
        }
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, storyId.ToString());
        string storySessionId = storyId.ToString();
        if (_storySessionService.SessionIsActive(storySessionId))
        {
            _storySessionService.RemoveConnectionFromSession(storySessionId, Context.ConnectionId);
            if (_storySessionService.SessionIsEmpty(storySessionId))
            {
                // TODO: remove timer
                _storySessionService.RemoveSession(storySessionId);
            }
        }
        
        await Clients.Group(storyId.ToString()).UserDisconnected(userName);
    }
    
    public async Task SendStoryPart(int storyId, string storyPartText)
    {
        CreateStoryPartDto newStoryPart = new()
        {
            Text = storyPartText
        };

        string? userName = Context.User?.Identity?.Name;

        if(userName is null)
        {
            await Clients.Caller.MessageFailed("Unable to send message. It wasn't possible to get logged user.");
            _logger.LogWarning("Non logged user tried to send message to story {StoryId}.", storyId);
            return;
        }

        StoryPartDto? createdStoryPart = await _storyService.CreateStoryPartAsync(storyId, userName, newStoryPart);
        if(createdStoryPart is null)
        {
            await Clients.Caller.MessageFailed("Unable to send message. User isn't in story.");
            _logger.LogWarning("User '{UserName}' tried to send message to story {StoryId} that he is not in.", userName, storyId);
            return;
        }
        
        await Clients.Group(storyId.ToString()).ReceiveStoryPart(storyId, createdStoryPart);
    }
    
    // TODO: block access from users
    public async Task SendTimerSeconds(int seconds, int storyId)
    {
        await Clients.Group(storyId.ToString()).ReceiveTimerSeconds(seconds);
    }

    public async Task OnConnectedAsync(int storyId)
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}