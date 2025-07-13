using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace api.Services;

public interface IStorySessionService : IDisposable
{
    int SessionsCount { get; }
    void AddSession(string storyId);
    void RemoveSession(string storyId);
    void RemoveAllSessions();
    bool SessionIsActive(string storyId);
    bool SessionIsEmpty(string storyId);
    void AddConnectionToSession(string storyId, string connectionId);
    void RemoveConnectionFromSession(string storyId, string connectionId);
    
    IReadOnlySet<string> GetSessionConnections(string storyId);
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
    private readonly ConcurrentDictionary<string, HashSet<string>> _sessions = new();
    private readonly ILogger<StorySessionService> _logger;
    public int SessionsCount { get => _sessions.Count; }
    
    public StorySessionService(ILogger<StorySessionService> logger)
    {
        _logger = logger;
    }

    public void AddSession(string storyId)
    {
        _sessions.TryAdd(storyId, new HashSet<string>());
        _logger.LogInformation("Added session for group {StoryId}", storyId);
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
        return _sessions[storyId].Count <= 0;
    }

    public void RemoveAllSessions()
    {
        _sessions.Clear();
        _logger.LogInformation("Removed all sessions");   
    }
    
    public void AddConnectionToSession(string storyId, string connectionId)
    {
        _sessions[storyId].Add(connectionId);
        _logger.LogInformation("Added connection {ConnectionId} to session for group {StoryId}", connectionId, storyId);
    }
    
    public void RemoveConnectionFromSession(string storyId, string connectionId)
    {
        _sessions[storyId].Remove(connectionId);
        _logger.LogInformation("Removed connection {ConnectionId} from session for group {StoryId}", connectionId, storyId);
    }
    
    /// <summary>
    /// Retrieves all connection IDs associated with a specified session.
    /// </summary>
    /// <param name="storyId">The identifier of the story session to retrieve connections from.</param>
    /// <returns>A read-only set of connection IDs for the specified session.</returns>
    public IReadOnlySet<string> GetSessionConnections(string storyId)
    {
        IReadOnlySet<string> connections = _sessions[storyId];
        
        _logger.LogDebug("Returning connections for group {StoryId}", storyId);
        return connections;   
    }

    public void Dispose()
    {
        RemoveAllSessions();
    }
}