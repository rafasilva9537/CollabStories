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
    Task UpdateAllTimersAsync(double deltaTimeSeconds);
    public int GetSessionTurnDurationSeconds(string storyId);
    public double GetSessionTimerSeconds(string storyId);
    public void DecrementSessionTimer(string storyId, double seconds = 1);
    void AddConnectionToSession(string storyId, string connectionId);
    void RemoveConnectionFromSession(string storyId, string connectionId);
    
    IReadOnlySet<string> GetSessionConnections(string storyId);
}