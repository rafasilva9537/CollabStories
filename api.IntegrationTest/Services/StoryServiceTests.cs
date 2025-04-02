using api.Data;
using api.Services;
using api.Dtos.Story;
using api.Models;
using api.IntegrationTests.Services.Data;
using Microsoft.EntityFrameworkCore;
using api.Mappers;

namespace api.IntegrationTests.Services;

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
        using ApplicationDbContext context = _testDatabase.CreateDbContext();
        await context.Database.BeginTransactionAsync();
        await context.Story.ExecuteDeleteAsync();
        StoryService storyService = new StoryService(context);

        //Act
        IList<StoryMainInfoDto> actualStories = await storyService.GetStoriesAsync();

        //Assert
        Assert.Empty(actualStories);
    }

    [Fact]
    public async Task GetStoriesAsync_WhenStoriesExists_ReturnsNonEmptyList()
    {
        //Arrange 
        using ApplicationDbContext context = _testDatabase.CreateDbContext();
        StoryService storyService = new StoryService(context);

        await context.Database.BeginTransactionAsync();
        Story newStory = new Story {
            Title = "Test story",
            Description = "This is a test story",
        };
        await context.Story.AddAsync(newStory);
        await context.SaveChangesAsync();

        //Act
        // TODO: see if ef tracker is caching data
        IList<StoryMainInfoDto> actualStories = await storyService.GetStoriesAsync();

        //Assert
        Assert.NotEmpty(actualStories);
    }

    [Fact]
    public async Task GetStoriesAsync_WhenOnlyOneStoryExists_ReturnsExpectedStoryValue()
    {
        //Arrange 
        using ApplicationDbContext context = _testDatabase.CreateDbContext();
        StoryService storyService = new StoryService(context);

        await context.Database.BeginTransactionAsync();
        await context.Story.ExecuteDeleteAsync();
        Story newStory = new Story {
            Title = "Test story",
            Description = "This is a test story",
        };
        StoryMainInfoDto expectedStory = newStory.ToStoryMainInfoDto();

        await context.Story.AddAsync(newStory);
        await context.SaveChangesAsync();

        //Act
        // TODO: see if ef tracker is caching data
        context.ChangeTracker.Clear();
        IList<StoryMainInfoDto> actualStories = await storyService.GetStoriesAsync();

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
}