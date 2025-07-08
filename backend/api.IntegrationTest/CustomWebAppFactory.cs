using System.Net.Http.Headers;
using api.Data;
using api.IntegrationTests.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace api.IntegrationTests;

public class CustomWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseEnvironment("Test");

        var configuration = new ConfigurationBuilder()
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

            // Dataseed
            using (var scope = services.BuildServiceProvider().CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                dbContext.Database.EnsureDeleted();
                bool dbCreated = dbContext.Database.EnsureCreated();

                if (dbCreated)
                {
                    SeedTestDatabase.Initialize(dbContext, 100);
                }
            }
        });
    }

    public HttpClient CreateClientWithAuth(string userName, string nameIdentifier, string email, params string[] roles)
    {
        HttpClient client = WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = TestAuthHandler.AuthenticationScheme;
                        options.DefaultChallengeScheme = TestAuthHandler.AuthenticationScheme;
                        options.DefaultScheme = TestAuthHandler.AuthenticationScheme;
                    })
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.AuthenticationScheme, options => { });
            });
        }).CreateClient();
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthHandler.AuthenticationScheme);
        client.DefaultRequestHeaders.Add(TestAuthHandler.NameHeader, userName);
        client.DefaultRequestHeaders.Add(TestAuthHandler.NameIdentifierHeader, nameIdentifier);
        client.DefaultRequestHeaders.Add(TestAuthHandler.EmailHeader, email);
        foreach (string role in roles)
        {
            client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);
        }

        return client;
    }
}
