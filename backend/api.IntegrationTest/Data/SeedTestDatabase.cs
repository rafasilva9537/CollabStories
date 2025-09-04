using api.Data;
using api.Data.Seed;
using api.Models;

namespace api.IntegrationTests.Data;

public static class SeedTestDatabase
{
    public static void Initialize(ApplicationDbContext dbContext, int totalUsers = 0)
    {
        SeedDatabase.Initialize(dbContext, totalUsers);

        string adminUserName = "test_admin";
        string adminEmail = $"{adminUserName}@gmail.com";
        AppUser testAdmin = new()
        {
            UserName = adminUserName,
            Email = adminEmail,
            NormalizedUserName = adminUserName.ToUpper(),
            NormalizedEmail = adminEmail.ToUpper(),
            NickName = "Test Admin",
            Description = "This is a test admin",
            CreatedDate = new DateTimeOffset(2022, 1, 1, 0, 0, 0, new TimeSpan(0)),
        };
        dbContext.AppUser.Add(testAdmin);
        
        dbContext.SaveChanges();
    }
}