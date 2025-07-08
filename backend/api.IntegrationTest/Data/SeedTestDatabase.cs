using api.Data;
using api.Data.Seed;
using api.Models;

namespace api.IntegrationTests.Data;

public static class SeedTestDatabase
{
    public static void Initialize(ApplicationDbContext dbContext, int totalUsers)
    {
        SeedDatabase.Initialize(dbContext, totalUsers);

        AppUser testAdmin = new()
        {
            UserName = "test_admin",
            Email = "test_admin@gmail.com",
            NormalizedUserName = "TEST_ADMIN",
            NormalizedEmail = "TEST_ADMIN@GMAIL.COM",
            Nickname = "Test Admin",
            Description = "This is a test admin",
            CreatedDate = new DateTimeOffset(2022, 1, 1, 0, 0, 0, new TimeSpan(0)),
            
        };
        dbContext.AppUser.Add(testAdmin);
        dbContext.SaveChanges();
    }
}