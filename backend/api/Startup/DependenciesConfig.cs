using api.Interfaces;
using api.Services;

namespace api.Startup;

public static class DependenciesConfig
{
    public static void AddDependencyInjectionServices(this IServiceCollection services)
    {
        services.AddScoped<IStoryService, StoryService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IImageService, ImageService>();
        
        services.AddTransient<IDateTimeProvider, DateTimeProvider>();
        
        services.AddSingleton<IStorySessionService, StorySessionService>();
        
        services.AddHostedService<TimerBackgroundService>();
    }
}