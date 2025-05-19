using api.Data;
using api.IntegrationTest.Services.Data;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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

        if (dbCreated)
        {
            FakeDataGenerator fakeData = new();

            int totalUsers = 300;
            int totalStories = 1000;
            int totalStoryParts = totalStories * 50;

            IDbContextTransaction seedTransaction = dbContext.Database.BeginTransaction();

            List<AppUser> newUsers = fakeData.GenerateAppUsers(totalUsers);
            dbContext.AppUser.AddRange(newUsers);
            dbContext.SaveChanges();
            newUsers = dbContext.AppUser.ToList();

            List<Story> newStories = fakeData.GenerateStories(totalStories, newUsers);
            dbContext.Story.AddRange(newStories);
            List<Story> storiesWithoutUser = fakeData.GenerateStories(totalStories/10);
            dbContext.Story.AddRange(storiesWithoutUser);
            dbContext.SaveChanges();
            newStories = dbContext.Story.ToList();

            List<StoryPart> storyParts = fakeData.GenerateStoryParts(totalStoryParts, newStories, newUsers);
            dbContext.StoryPart.AddRange(storyParts);
            List<StoryPart> storyPartsWithoutUsers = fakeData.GenerateStoryParts(totalStoryParts/100, newStories);
            dbContext.StoryPart.AddRange(storyPartsWithoutUsers);
            dbContext.SaveChanges();

            seedTransaction.Commit();
        }
    }
}