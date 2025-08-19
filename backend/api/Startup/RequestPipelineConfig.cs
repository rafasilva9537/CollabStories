using api.Data;
using api.Data.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace api.Startup;

public static class RequestPipelineConfig
{
    public static async Task<WebApplication> StartDevDatabase(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
        
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
        await SeedRoles.InitializeAsync(roleManager);
        
        if (!await dbContext.AppUser.AnyAsync())
        {
            SeedDatabase.Initialize(dbContext, 100);
        }

        return app;
    }
}