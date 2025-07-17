using System.Collections.Concurrent;
using api.Dtos.Story;
using api.Exceptions;
using api.Interfaces;

namespace api.Services;

public interface IStorySessionService : IDisposable
{
    int SessionsCount { get; }
    Task AddSessionAsync(string storyId, IStoryService storyService);
    void RemoveSession(string storyId);
    void RemoveAllSessions();
    bool SessionIsActive(string storyId);
    bool SessionIsEmpty(string storyId);
    public int GetSessionTurnDurationSeconds(string storyId);
    public double GetSessionTimerSeconds(string storyId);
    public void DecrementSessionTimer(string storyId, double seconds = 1);
    void AddConnectionToSession(string storyId, string connectionId);
    void RemoveConnectionFromSession(string storyId, string connectionId);
    
    IReadOnlySet<string> GetSessionConnections(string storyId);
}

public class SessionInfo
{
    public readonly HashSet<string> Connections = [];
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

    public int SessionsCount { get => _sessions.Count; }
    
    public StorySessionService(ILogger<IStorySessionService> logger, IDateTimeProvider dateTimeProvider)
    {
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task AddSessionAsync(string storyId, IStoryService storyService)
    {
        StoryInfoForSessionDto? story = await storyService.GetStoryInfoForSessionAsync(int.Parse(storyId));
        if (story is null)
        {
            throw new NoStoryException($"Story with ID {storyId} not found.");
        }

        TimeSpan timeSinceUpdate = _dateTimeProvider.UtcNow - story.AuthorsMembershipChangeDate;
        double remainingTimerSeconds = timeSinceUpdate.TotalSeconds;
        int turnDurationSeconds = story.TurnDurationSeconds;
        
        SessionInfo sessionInfo = new SessionInfo(remainingTimerSeconds, turnDurationSeconds);
        _sessions.TryAdd(storyId, sessionInfo);
        
        _logger.LogInformation(
            "Added session for group {StoryId}, with timer starting at {TimerSeconds} seconds and turn duration of {TurnDurationSeconds} seconds.",
            storyId,
            remainingTimerSeconds,
            turnDurationSeconds);
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
        return _sessions[storyId].Connections.Count <= 0;
    }

    public void RemoveAllSessions()
    {
        _sessions.Clear();
        _logger.LogInformation("Removed all sessions");   
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
        _sessions[storyId].Connections.Add(connectionId);
        _logger.LogInformation("Added connection {ConnectionId} to session for group {StoryId}", connectionId, storyId);
    }
    
    public void RemoveConnectionFromSession(string storyId, string connectionId)
    {
        _sessions[storyId].Connections.Remove(connectionId);
        _logger.LogInformation("Removed connection {ConnectionId} from session for group {StoryId}", connectionId, storyId);
    }
    
    /// <summary>
    /// Retrieves all connection IDs associated with a specified session.
    /// </summary>
    /// <param name="storyId">The identifier of the story session to retrieve connections from.</param>
    /// <returns>A read-only set of connection IDs for the specified session.</returns>
    public IReadOnlySet<string> GetSessionConnections(string storyId)
    {
        IReadOnlySet<string> connections = _sessions[storyId].Connections;
        
        _logger.LogDebug("Returning connections for group {StoryId}", storyId);
        return connections;
    }

    public void Dispose()
    {
        RemoveAllSessions();
    }
}