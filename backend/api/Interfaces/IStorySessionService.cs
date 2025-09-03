using System.Collections.Concurrent;
using api.Services;

namespace api.Interfaces;

public interface IStorySessionService : IDisposable
{
    int SessionsCount { get; }
    ICollection<string> GetSessionIds();
    Task AddSessionAsync(string storyId, IStoryService storyService);
    void RemoveSession(string storyId);
    void RemoveAllSessions();
    bool SessionIsActive(string storyId);
    bool SessionIsEmpty(string storyId);
    Task UpdateAllTimersAsync();
    DateTimeOffset GetTurnEndTime(string storyId);
    void AddConnectionToSession(string storyId, string connectionId);
    void RemoveConnectionFromSession(string storyId, string connectionId);
    
    IReadOnlySet<string> GetSessionConnections(string storyId);
}