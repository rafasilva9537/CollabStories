using api.Data;
using api.Services;
using api.Dtos.Story;
using Xunit.Abstractions;
using api.Models;

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
        // TODO: implement all stories deletion without commit
        using ApplicationDbContext context = _testDatabase.CreateDbContext();
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

        context.Story.Remove(newStory);
        await context.SaveChangesAsync();
    }

    [Fact]
    public void GetStoriesAsync_WhenSingleStoryExists_ReturnsExpectedSingleStoryValue()
    {
        // TODO: implement
    }
}