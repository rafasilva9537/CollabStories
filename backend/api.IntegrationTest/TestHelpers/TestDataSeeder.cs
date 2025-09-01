using api.Data;
using api.Models;

namespace api.IntegrationTests.TestHelpers;

internal static class TestDataSeeder
{
    public static void SeedUser(ApplicationDbContext dbContext, AppUser user)
    {
        dbContext.AppUser.Add(user);
        dbContext.SaveChanges();
    }

    public static void SeedMultipleUsers(ApplicationDbContext dbContext, List<AppUser> users)
    {
        dbContext.AppUser.AddRange(users);
        dbContext.SaveChanges();
    }

    public static void SeedStory(ApplicationDbContext dbContext, Story story)
    {
        dbContext.Story.Add(story);
        dbContext.SaveChanges();
    }

    public static void SeedMultipleStories(ApplicationDbContext dbContext, List<Story> stories)
    {
        dbContext.Story.AddRange(stories);
        dbContext.SaveChanges();   
    }
    
    public static void SeedAuthorInStory(ApplicationDbContext dbContext, AuthorInStory authorInStory)
    {
        dbContext.AuthorInStory.Add(authorInStory);
        dbContext.SaveChanges();
    }
}