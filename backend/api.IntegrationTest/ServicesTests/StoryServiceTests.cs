using api.Data;
using api.Dtos.Story;
using api.IntegrationTests.Constants;
using api.IntegrationTests.ServicesTests.ServicesFixtures;
using api.IntegrationTests.TestHelpers;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace api.IntegrationTests.ServicesTests;

[Collection(CollectionConstants.IntegrationTestsDatabase)]
public class StoryServiceTests : IClassFixture<StoryServiceFixture>
{
    private readonly StoryServiceFixture _fixture;

    public StoryServiceTests(StoryServiceFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetStoriesAsync_WhenNoStoryExists_ReturnsEmptyList()
    {
        //Arrange
        using IServiceScope scope = _fixture.Factory.Services.CreateScope();
        IStoryService storyService = scope.ServiceProvider.GetRequiredService<IStoryService>();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Database.BeginTransactionAsync();
        await dbContext.Story.ExecuteDeleteAsync();

        //Act
        var actualStories = await storyService.GetStoriesAsync(0);

        //Assert
        Assert.Empty(actualStories.Items);
    }

    [Fact]
    public async Task GetStoriesAsync_WhenStoriesExists_ReturnsNonEmptyList()
    {
        //Arrange 
        using IServiceScope scope = _fixture.Factory.Services.CreateScope();
        IStoryService storyService = scope.ServiceProvider.GetRequiredService<IStoryService>();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Database.BeginTransactionAsync();
        Story newStory = TestModelFactory.CreateStoryModel(userId: _fixture.DefaultAppUser.Id);
        await dbContext.Story.AddAsync(newStory);
        await dbContext.SaveChangesAsync();

        //Act
        var actualStories = await storyService.GetStoriesAsync(16);

        //Assert
        Assert.NotEmpty(actualStories.Items);
    }

    [Theory]
    [InlineData("Test story", "This is a test story")]
    [InlineData("Story without description", "")]
    public async Task GetStoriesAsync_WhenOnlyOneStoryExists_ReturnsExpectedStoryValue(string storyTitle, string storyDescription)
    {
        //Arrange 
        using IServiceScope scope = _fixture.Factory.Services.CreateScope();
        IStoryService storyService = scope.ServiceProvider.GetRequiredService<IStoryService>();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Database.BeginTransactionAsync();
        await dbContext.Story.ExecuteDeleteAsync();

        AppUser defaultUser = _fixture.DefaultAppUser;
        Story expectedStory = TestModelFactory.CreateStoryModel(storyTitle, storyDescription, userId: defaultUser.Id);
        TestDataSeeder.SeedStory(dbContext, expectedStory);

        //Act
        var actualStories = await storyService.GetStoriesAsync(null);

        //Assert
        Assert.Single(actualStories.Items);
        
        StoryMainInfoDto firstStory = actualStories.Items[0];
        Assert.NotEqual(0, firstStory.Id);
        Assert.Equal(expectedStory.Title, firstStory.Title);
        Assert.Equal(expectedStory.Description, firstStory.Description);
        Assert.Equal(defaultUser.UserName, firstStory.UserName);
        Assert.Equal(expectedStory.MaximumAuthors, firstStory.MaximumAuthors);
        Assert.Equal(expectedStory.CreatedDate, firstStory.CreatedDate);
        Assert.Equal(expectedStory.UpdatedDate, firstStory.UpdatedDate);
    }

    [Theory]
    [InlineData("My new test story")]
    [InlineData("")]
    public async Task StoryExists_WhenStoryExists_ReturnsTrue(string storyTitle)
    {
        // Arrange
        using IServiceScope scope = _fixture.Factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        IStoryService storyService = scope.ServiceProvider.GetRequiredService<IStoryService>();

        AppUser defaultUser = _fixture.DefaultAppUser;
        Story story = TestModelFactory.CreateStoryModel(storyTitle, userId: defaultUser.Id);
        TestDataSeeder.SeedStory(dbContext, story);

        // Act
        bool storyExists = await storyService.StoryExistsAsync(story.Id);

        // Assert 
        Assert.True(storyExists);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10_000)]
    [InlineData(20_000)]
    public async Task StoryExists_WhenStoryDoesNotExist_ReturnsFalse(int storyId)
    {
        // Arrange 
        using IServiceScope scope = _fixture.Factory.Services.CreateScope();
        IStoryService storyService = scope.ServiceProvider.GetRequiredService<IStoryService>();
        scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Act
        bool storyExists = await storyService.StoryExistsAsync(storyId);

        // Assert
        Assert.False(storyExists);
    }

    [Fact]
    public async Task GetStoryInfoForSession_WhenStoryExists_ReturnsStoryInfoForSessionDto()
    {
        // Arrange 
        using IServiceScope scope = _fixture.Factory.Services.CreateScope();
        IStoryService storyService = scope.ServiceProvider.GetRequiredService<IStoryService>();
        Story expectedStory = _fixture.DefaultStory;

        // Act
        StoryInfoForSessionDto? storyInfo = await storyService.GetStoryInfoForSessionAsync(expectedStory.Id);

        // Assert
        Assert.NotNull(storyInfo);
        Assert.Equal(expectedStory.TurnDurationSeconds, storyInfo.TurnDurationSeconds);
        Assert.Equal(expectedStory.UpdatedDate, storyInfo.AuthorsMembershipChangeDate);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(int.MaxValue)]
    public async Task GetStoryInfoForSession_WhenStoryDoesNotExist_ReturnsNull(int storyId)
    {
        // Arrange
        using IServiceScope scope = _fixture.Factory.Services.CreateScope();
        IStoryService storyService = scope.ServiceProvider.GetRequiredService<IStoryService>();

        // Act
        StoryInfoForSessionDto? storyInfo = await storyService.GetStoryInfoForSessionAsync(storyId);

        // Assert
        Assert.Null(storyInfo);
    }

    [Fact]
    public async Task ChangeToNextCurrentAuthorAsync_WithThreeAuthors_CyclesThroughAuthorsCorrectly()
    {
        // Arrange 
        using IServiceScope scope = _fixture.Factory.Services.CreateScope();
        IStoryService storyService = scope.ServiceProvider.GetRequiredService<IStoryService>();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        AppUser storyOwner = TestModelFactory.CreateAppUserModel(
            baseUserName: "storyowner",
            baseEmail: "owner@example.com",
            nickname: "StoryOwner");
        TestDataSeeder.SeedUser(dbContext, storyOwner);
        
        AppUser secondAuthor = TestModelFactory.CreateAppUserModel(
            baseUserName: "secondauthor", 
            baseEmail: "second@example.com",
            nickname: "SecondAuthor");
        TestDataSeeder.SeedUser(dbContext, secondAuthor);
        
        AppUser thirdAuthor = TestModelFactory.CreateAppUserModel(
            baseUserName: "thirdauthor", 
            baseEmail: "third@example.com",
            nickname: "ThirdAuthor");
        TestDataSeeder.SeedUser(dbContext, thirdAuthor);
        
        Story testStory = TestModelFactory.CreateStoryModel(
            title: "Test Story with Multiple Authors",
            description: "A story to test cycling through multiple authors",
            userId: storyOwner.Id,
            currentAuthorId: storyOwner.Id,
            createdDate: new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            updatedDate: new DateTimeOffset(2025, 1, 15, 0, 0, 0, TimeSpan.Zero));
        TestDataSeeder.SeedStory(dbContext, testStory);
        
        DateTimeOffset baseTime = testStory.UpdatedDate;
        AuthorInStory ownerAsAuthor = TestModelFactory.CreateAuthorInStory(
            storyId: testStory.Id,
            userId: storyOwner.Id,
            entryDate: baseTime);
        TestDataSeeder.SeedAuthorInStory(dbContext, ownerAsAuthor);

        AuthorInStory secondAuthorInStory = TestModelFactory.CreateAuthorInStory(
            storyId: testStory.Id,
            userId: secondAuthor.Id,
            entryDate: baseTime.AddMinutes(5));
        TestDataSeeder.SeedAuthorInStory(dbContext, secondAuthorInStory);

        AuthorInStory thirdAuthorInStory = TestModelFactory.CreateAuthorInStory(
            storyId: testStory.Id,
            userId: thirdAuthor.Id,
            entryDate: baseTime.AddMinutes(10));
        TestDataSeeder.SeedAuthorInStory(dbContext, thirdAuthorInStory);

        
        // Act/Assert
        // Initially the current author should be the story owner
        Story? initialStory = await dbContext.Story
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == testStory.Id);
        Assert.NotNull(initialStory);
        Assert.Equal(storyOwner.Id, initialStory.CurrentAuthorId);
        
        // Change to second author
        string returnedSecondAuthorName = await storyService.ChangeToNextCurrentAuthorAsync(testStory.Id);
        Story? afterFirstChange = await dbContext.Story
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == testStory.Id);
        Assert.NotNull(afterFirstChange);
        Assert.Equal(secondAuthor.Id, afterFirstChange.CurrentAuthorId);
        Assert.Equal(secondAuthor.UserName, returnedSecondAuthorName);
        
        // Change to third author
        string returnedThirdAuthorName = await storyService.ChangeToNextCurrentAuthorAsync(testStory.Id);
        Story? afterSecondChange = await dbContext.Story
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == testStory.Id);
        Assert.NotNull(afterSecondChange);
        Assert.Equal(thirdAuthor.Id, afterSecondChange.CurrentAuthorId);
        Assert.Equal(thirdAuthor.UserName, returnedThirdAuthorName);
        
        // Change back to the first author
        string returnedOwnerName = await storyService.ChangeToNextCurrentAuthorAsync(testStory.Id);
        Story? afterThirdChange = await dbContext.Story
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == testStory.Id);
        Assert.NotNull(afterThirdChange);
        Assert.Equal(storyOwner.Id, afterThirdChange.CurrentAuthorId);
        Assert.Equal(storyOwner.UserName, returnedOwnerName);
    }
}