using api.Constants;
using api.Data;
using api.IntegrationTests.TestHelpers;
using api.IntegrationTests.WebAppFactories;
using api.Models;
using Microsoft.Extensions.DependencyInjection;

namespace api.IntegrationTests.ControllersTests.ControllersFixtures;

public class AccountControllerFixture
{
    public AuthHandlerWebAppFactory Factory { get; }
    public AppUser DefaultUser { get; }
    
    public AccountControllerFixture()
    {
        Factory = new AuthHandlerWebAppFactory();
        
        using IServiceScope scope = Factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        DefaultUser = TestModelFactory.CreateAppUserModel(
            "john_doe", 
            "john.doe@example.com", 
            "John Doe", 
            "Software developer and tech enthusiast", 
            Path.Combine(
                DirectoryPathConstants.Media,
                DirectoryPathConstants.Images,
                DirectoryPathConstants.ProfileImages, "john_doe.jpg"), 
            DateTimeOffset.Now);
        TestDataSeeder.SeedUser(dbContext, DefaultUser);
    }
}