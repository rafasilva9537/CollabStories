using api.Data;
using api.Data.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace api.IntegrationTests.Data;

// Fixture used when only db connection is needed,
// not the whole Program under test
public class TestDatabaseFixture
{
    private static readonly object Lock = new();
    private static bool _databaseInitialized;

    public TestDatabaseFixture()
    {
        if(!_databaseInitialized)
        {
            lock(Lock)
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
        var configuration = builder.Build();

        string? connectionString = configuration["ConnectionStrings:DbTestConnection"];

        if(string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentNullException("DbTestConnection connection string is not set in configuration");
        }

        var dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(connectionString)
        .Options;

        var dbContextTest = new ApplicationDbContext(dbContextOptions);
        return dbContextTest;
    }

    private void InitializeDatabase()
    {
        using var dbContext = CreateDbContext();

        dbContext.Database.EnsureDeleted();
        bool dbCreated = dbContext.Database.EnsureCreated();

        if (dbCreated)
        {
            SeedDatabase.Initialize(dbContext, 100);
        }
    }
}