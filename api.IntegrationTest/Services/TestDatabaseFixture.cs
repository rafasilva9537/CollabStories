using System.Reflection;
using api.Data;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace api.IntegrationTests.Services;

// Fixture used when only db connection is needed,
// not the whole Program under test
public class TestDatabaseFixture
{
    private static readonly object _lock = new();
    private static bool _databaseInitialized;
    //private readonly IConfiguration _configuration;

    public TestDatabaseFixture()
    {
        if(!_databaseInitialized)
        {
            lock(_lock)
            {
                InitializeDatabase();
                _databaseInitialized = true;
            }
        }
    }

    public ApplicationDbContext CreateDbContext()
    {
        var builder = new ConfigurationBuilder()
            .AddUserSecrets<Program>();
        var _configuration = builder.Build();

        string? connectionString = _configuration["ConnectionStrings:DbTestConnection"];

        if(connectionString is null)
        {
            throw new ArgumentNullException("DbTestConnection connection string is not set in configuration");
        }

        var dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(connectionString)
        .Options;

        var dbContextTest = new ApplicationDbContext(dbContextOptions);
        return dbContextTest;
    }

    public void InitializeDatabase()
    {
        using var dbContext = CreateDbContext();

        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();

        List<AppUser> newUsers = [
            new AppUser
                {
                    UserName = "bob",
                    Email = "user@email.example"
                },
                new AppUser
                {
                    UserName = "joe",
                    Email = "user@email.example"
                },
                new AppUser
                {
                    UserName = "dude_25",
                    Email = "user@email.example"
                },
            ];

        dbContext.AppUser.AddRange(newUsers);
        dbContext.SaveChanges();
    }
}