using api.Interfaces;

namespace api.Services;

public class TimerBackgroundService : BackgroundService
{
    private readonly IStorySessionService _storySessionService;
    
    public TimerBackgroundService(IStorySessionService storySessionService)
    {
        _storySessionService = storySessionService;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const int deltaTimeSeconds = 1;
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(deltaTimeSeconds));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await _storySessionService.UpdateAllTimersAsync();
        }
    }
}