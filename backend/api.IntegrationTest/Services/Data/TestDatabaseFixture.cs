using api.Data;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace api.IntegrationTests.Services.Data;

// Fixture used when only db connection is needed,
// not the whole Program under test
public class TestDatabaseFixture
{
    private static readonly object _lock = new();
    private static bool _databaseInitialized;

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

        if(string.IsNullOrEmpty(connectionString))
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

        // TODO: remove db deletion after implementing whole database seed
        dbContext.Database.EnsureDeleted();
        bool dbCreated = dbContext.Database.EnsureCreated();

        if(dbCreated)
        {
            List<AppUser> newUsers = [
                new AppUser
                {
                    UserName = "bob",
                    Email = "user@email1.example"
                },
                new AppUser
                {
                    UserName = "joe",
                    Email = "user@email2.example"
                },
                new AppUser
                {
                    UserName = "dude_25",
                    Email = "user@email3.example"
                },
            ];

            List<Story> newStories = [
                new Story
                {
                    Title = "Story1",
                    Description = "Story1 description",
                    MaximumAuthors = 4,
                    TurnDurationSeconds = 120,
                    UserId = 1,
                },
                new Story
                {
                    Title = "Story2",
                    Description = "Story2 description",
                    MaximumAuthors = 4,
                    TurnDurationSeconds = 120,
                    UserId = 1,
                },
                new Story
                {
                    Title = "Story3",
                    Description = "Story3 description",
                    MaximumAuthors = 4,
                    TurnDurationSeconds = 120,
                    UserId = 2,
                },
            ];

            dbContext.AppUser.AddRange(newUsers);
            dbContext.Story.AddRange(newStories);
            dbContext.SaveChanges();
        }
    }
}