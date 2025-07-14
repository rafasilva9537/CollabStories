using api.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace api.Data.Seed;

public static class SeedDatabase
{
    public static void Initialize(ApplicationDbContext dbContext, int totalUsers = 300)
    {
        FakeDataGenerator fakeData = new();

        int totalStories = totalUsers * 10;
        int totalStoryParts = totalStories * 50;

        IDbContextTransaction seedTransaction = dbContext.Database.BeginTransaction();

        List<AppUser> newUsers = fakeData.GenerateAppUsers(totalUsers);
        dbContext.AppUser.AddRange(newUsers);
        dbContext.SaveChanges();
        newUsers = dbContext.AppUser.ToList();

        List<Story> newStories = fakeData.GenerateStories(totalStories, newUsers);
        dbContext.Story.AddRange(newStories);
        dbContext.SaveChanges();
        newStories = dbContext.Story.ToList();

        List<StoryPart> storyParts = fakeData.GenerateStoryParts(totalStoryParts, newStories, newUsers);
        dbContext.StoryPart.AddRange(storyParts);
        dbContext.SaveChanges();
        
        seedTransaction.Commit();
    }
}