using api.Data;
using api.Dtos.Story;
using api.IntegrationTests.Constants;
using api.IntegrationTests.Data;
using api.Mappers;
using api.Models;
using api.Services;
using Microsoft.EntityFrameworkCore;

namespace api.IntegrationTests.ServicesTests;

[Collection(CollectionConstants.IntegrationTestsDatabase)]
public class StoryServiceTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _testDatabase;
    public StoryServiceTests(TestDatabaseFixture testDatabase)
    {
        _testDatabase = testDatabase;
    }

    [Fact]
    public async Task GetStoriesAsync_WhenNoStoryExists_ReturnsEmptyList()
    {
        //Arrange
        await using ApplicationDbContext context = _testDatabase.CreateDbContext();
        await context.Database.BeginTransactionAsync();
        await context.Story.ExecuteDeleteAsync();
        StoryService storyService = new StoryService(context);

        //Act
        IList<StoryMainInfoDto> actualStories = await storyService.GetStoriesAsync(0);

        //Assert
        Assert.Empty(actualStories);
    }

    [Fact]
    public async Task GetStoriesAsync_WhenStoriesExists_ReturnsNonEmptyList()
    {
        //Arrange 
        await using ApplicationDbContext context = _testDatabase.CreateDbContext();
        StoryService storyService = new StoryService(context);

        await context.Database.BeginTransactionAsync();
        Story newStory = new Story
        {
            Title = "Test story",
            Description = "This is a test story",
        };
        await context.Story.AddAsync(newStory);
        await context.SaveChangesAsync();

        //Act
        // TODO: see if ef tracker is caching data
        IList<StoryMainInfoDto> actualStories = await storyService.GetStoriesAsync(16);

        //Assert
        Assert.NotEmpty(actualStories);
    }

    [Theory]
    [InlineData("Test story", "This is a test story")]
    [InlineData("Story without description", "")]
    public async Task GetStoriesAsync_WhenOnlyOneStoryExists_ReturnsExpectedStoryValue(string title, string description)
    {
        //Arrange 
        await using ApplicationDbContext context = _testDatabase.CreateDbContext();
        StoryService storyService = new StoryService(context);

        await context.Database.BeginTransactionAsync();
        await context.Story.ExecuteDeleteAsync();
        Story newStory = new Story
        {
            Title = title,
            Description = description,
        };
        StoryMainInfoDto expectedStory = newStory.ToStoryMainInfoDto();

        await context.Story.AddAsync(newStory);
        await context.SaveChangesAsync();

        //Act
        // TODO: see if ef tracker is caching data
        context.ChangeTracker.Clear();
        IList<StoryMainInfoDto> actualStories = await storyService.GetStoriesAsync(null);

        //Assert
        Assert.True(actualStories.Count == 1);
        Assert.NotEqual(0, actualStories[0].Id);
        Assert.Equal(expectedStory.Title, actualStories[0].Title);
        Assert.Equal(expectedStory.Description, actualStories[0].Description);
        Assert.Equal(expectedStory.UserName, actualStories[0].UserName);
        Assert.Equal(expectedStory.MaximumAuthors, actualStories[0].MaximumAuthors);
        Assert.Equal(expectedStory.CreatedDate, actualStories[0].CreatedDate);
        Assert.Equal(expectedStory.UpdatedDate, actualStories[0].UpdatedDate);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(15)]
    [InlineData(500)]
    public async Task StoryExists_WhenStoryExists_ReturnsTrue(int storyId)
    {
        // Arrange 
        await using ApplicationDbContext context = _testDatabase.CreateDbContext();
        StoryService storyService = new StoryService(context);
        
        // Act
        bool storyExists = await storyService.StoryExistsAsync(storyId);
        
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
        await using ApplicationDbContext context = _testDatabase.CreateDbContext();
        StoryService storyService = new StoryService(context);
        
        // Act
        bool storyExists = await storyService.StoryExistsAsync(storyId);
        
        // Assert
        Assert.False(storyExists);   
    }
}