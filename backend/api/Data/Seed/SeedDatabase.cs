using System.Threading.Tasks;
using api.Models;
using api.Services.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace api.Data.Seed;

public class SeedDatabase
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