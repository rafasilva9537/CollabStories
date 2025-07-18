using System.Net.Http.Headers;
using api.Constants;
using api.Data;
using api.Data.Seed;
using api.IntegrationTests.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace api.IntegrationTests.WebAppFactories;

/// <summary>
/// CustomWebAppFactory is a test-specific implementation of the
/// WebApplicationFactory with a test database
/// </summary>
public class CustomWebAppFactory : WebApplicationFactory<Program>
{
    public CustomWebAppFactory()
    {
        InitializeDatabase();
    }
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseEnvironment("Test");

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        // Used because builder.UseEnvironment("Test"); remove Program.cs access to user secrets
        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            configBuilder.AddConfiguration(configuration);
        });
        
        string? connectionString = configuration.GetConnectionString("DbTestConnection");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });
        });
    }
    
    private void InitializeDatabase()
    {
        // Dataseed
        using ApplicationDbContext dbContext = Services.GetRequiredService<ApplicationDbContext>();

        dbContext.Database.EnsureDeleted();
        bool dbCreated = dbContext.Database.EnsureCreated();

        if (dbCreated)
        {
            var roleManager = Services.GetRequiredService<RoleManager<IdentityRole<int>>>();
            SeedRoles.InitializeAsync(roleManager).Wait();
            
            SeedTestDatabase.Initialize(dbContext, 100);
        }
    }

    public HubConnection CreateHubConnection()
    {
        TestServer server = Server;
        HubConnection connection = new HubConnectionBuilder()
            .WithUrl(server.BaseAddress + UrlConstants.StoryHub, options =>
            {
                options.HttpMessageHandlerFactory = _ => server.CreateHandler();
            })
            .Build();

        return connection;
    }
}
