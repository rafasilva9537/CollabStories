using System.Collections.Concurrent;
using api.Dtos.Story;
using api.Exceptions;
using api.Hubs;
using api.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace api.Services;

public class SessionInfo
{
    public readonly ConcurrentDictionary<string, byte> Connections = new();
    public double TimerSeconds { get; set; }
    public int TurnDurationSeconds { get; init; }
    
    public SessionInfo(double timerSeconds, int turnDurationSeconds)
    {
        TimerSeconds = timerSeconds;
        TurnDurationSeconds = turnDurationSeconds;
    }
}

/// <summary>
/// Represents a service for managing story sessions associated with groups ids (signalr group name).
/// Tracks and maintains the state of active sessions.
/// </summary>
public class StorySessionService : IStorySessionService
{
    /// <summary>
    /// A thread-safe dictionary that manages active session groups and their associated users' connection ids.
    /// Story id need to be string to be compatible with signalr group names, which are string
    /// </summary>
    private readonly ConcurrentDictionary<string, SessionInfo> _sessions = new();
    private readonly ILogger<IStorySessionService> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IHubContext<StoryHub, IStoryClient> _hubContext;
    
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public int SessionsCount { get => _sessions.Count; }
    
    public StorySessionService(
        ILogger<IStorySessionService> logger, 
        IDateTimeProvider dateTimeProvider, 
        IHubContext<StoryHub, IStoryClient> hubContext,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
        _hubContext = hubContext;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task AddSessionAsync(string storyId, IStoryService storyService)
    {
        StoryInfoForSessionDto? story = await storyService.GetStoryInfoForSessionAsync(int.Parse(storyId));
        if (story is null)
        {
            throw new StoryNotFoundException($"Story with ID {storyId} not found.");
        }

        TimeSpan timeSinceUpdate = _dateTimeProvider.UtcNow - story.AuthorsMembershipChangeDate;

        double remainingTimerSeconds = timeSinceUpdate.TotalSeconds;
        if (remainingTimerSeconds > story.TurnDurationSeconds)
        {
            remainingTimerSeconds %= story.TurnDurationSeconds;
        }
        
        int turnDurationSeconds = story.TurnDurationSeconds;
        
        SessionInfo sessionInfo = new SessionInfo(remainingTimerSeconds, turnDurationSeconds);
        _sessions.TryAdd(storyId, sessionInfo);
        
        _logger.LogInformation(
            "Added session for group {StoryId}, with timer starting at {TimerSeconds} seconds and turn duration of {TurnDurationSeconds} seconds.",
            storyId,
            remainingTimerSeconds,
            turnDurationSeconds);
    }
    
    public ICollection<string> GetSessionIds()
    {
        return _sessions.Keys;
    }
    
    public void RemoveSession(string storyId)
    {
        _sessions.TryRemove(storyId, out _);
        _logger.LogInformation("Removed session for group {StoryId}", storyId);
    }
    
    public bool SessionIsActive(string storyId)
    {
        return _sessions.ContainsKey(storyId);
    }

    public bool SessionIsEmpty(string storyId)
    {
        return _sessions[storyId].Connections.IsEmpty;
    }

    public void RemoveAllSessions()
    {
        _sessions.Clear();
        _logger.LogInformation("Removed all sessions");   
    }

    public async Task UpdateAllTimersAsync(double deltaTimeSeconds)
    {
        foreach (string storyId in _sessions.Keys)
        {
            if (_sessions[storyId].TimerSeconds <= 0)
            {
                using IServiceScope scope = _serviceScopeFactory.CreateScope();
                IStoryService storyService = scope.ServiceProvider.GetRequiredService<IStoryService>();
                await storyService.ChangeToNextCurrentAuthorAsync(int.Parse(storyId));
                
                _sessions[storyId].TimerSeconds = _sessions[storyId].TurnDurationSeconds;
            }
            
            await _hubContext.Clients.Group(storyId).ReceiveTimerSeconds(_sessions[storyId].TimerSeconds);
            DecrementSessionTimer(storyId, deltaTimeSeconds);
        }
    }

    public int GetSessionTurnDurationSeconds(string storyId)
    {
        return _sessions[storyId].TurnDurationSeconds;
    }

    public double GetSessionTimerSeconds(string storyId)
    {
        return _sessions[storyId].TimerSeconds;
    }

    public void DecrementSessionTimer(string storyId, double seconds = 1)
    {
        _sessions[storyId].TimerSeconds -= seconds;
    }
    
    public void AddConnectionToSession(string storyId, string connectionId)
    {
        _sessions[storyId].Connections.TryAdd(connectionId, 0);
        _logger.LogInformation("Added connection {ConnectionId} to session for group {StoryId}", connectionId, storyId);
    }
    
    public void RemoveConnectionFromSession(string storyId, string connectionId)
    {
        _sessions[storyId].Connections.TryRemove(connectionId, out _);
        _logger.LogInformation("Removed connection {ConnectionId} from session for group {StoryId}", connectionId, storyId);
    }
    
    /// <summary>
    /// Retrieves all connection IDs associated with a specified session.
    /// </summary>
    /// <param name="storyId">The identifier of the story session to retrieve connections from.</param>
    /// <returns>A read-only set of connection IDs for the specified session.</returns>
    public IReadOnlySet<string> GetSessionConnections(string storyId)
    {
        IReadOnlySet<string> connections = _sessions[storyId].Connections.Keys.ToHashSet();
        
        _logger.LogDebug("Returning connections for group {StoryId}", storyId);
        return connections;
    }

    public void Dispose()
    {
        RemoveAllSessions();
    }
}