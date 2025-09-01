using api.Data;
using api.Data.Seed;
using api.IntegrationTests.TestHelpers;
using api.IntegrationTests.WebAppFactories;
using api.Models;
using Microsoft.Extensions.DependencyInjection;

namespace api.IntegrationTests.ControllersTests.ControllersFixtures;

public class StoryControllerFixture
{
    public CustomWebAppFactory Factory { get; } 

    public StoryControllerFixture()
    {
        Factory = new CustomWebAppFactory();
        IServiceScope scope = Factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        FakeDataGenerator fakeDataGenerator = new();

        List<AppUser> users = TestModelFactory.CreateMultipleAppUserModels(5);
        TestDataSeeder.SeedMultipleUsers(dbContext, users);
        TestDataSeeder.SeedMultipleStories(dbContext, fakeDataGenerator.GenerateStories(65));
    }
}