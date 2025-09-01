using api.Data;
using api.IntegrationTests.TestHelpers;
using api.IntegrationTests.WebAppFactories;
using api.Models;
using Microsoft.Extensions.DependencyInjection;

namespace api.IntegrationTests.ServicesTests.ServicesFixtures;

public class StoryServiceFixture
{
    public CustomWebAppFactory Factory { get; }
    public Story DefaultStory { get; }
    public AppUser DefaultAppUser { get; }

    public StoryServiceFixture()
    {
        Factory = new CustomWebAppFactory();
        using IServiceScope scope = Factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        DefaultAppUser = TestModelFactory.CreateAppUserModel(
            baseUserName: "johndoe123",
            baseEmail: "john.doe@gmail.com",
            nickname: "JohnnyD",
            description: "Software developer passionate about coding and new technologies",
            createdDate: new DateTimeOffset(2023, 8, 15, 9, 30, 0, new TimeSpan(0)));
        TestDataSeeder.SeedUser(dbContext, DefaultAppUser);
        
        DefaultStory = TestModelFactory.CreateStoryModel(
            title: "The Magic Forest Adventure",
            description: "An enchanting tale about a group of friends discovering a mysterious forest filled with magical creatures and ancient secrets.",
            createdDate: new DateTimeOffset(2025, 6, 15, 14, 30, 0, new TimeSpan(0)),
            updatedDate: new DateTimeOffset(2025, 6, 15, 15, 45, 0, new TimeSpan(0)), 
            turnDurationSeconds: 1_800,
            maximumAuthors: 5,
            isFinished: false,
            userId: DefaultAppUser.Id,
            currentAuthorId: DefaultAppUser.Id);
        
        TestDataSeeder.SeedStory(dbContext, DefaultStory);
    }
}