using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text.Json;
using api.Dtos.Pagination;
using api.Dtos.Story;
using api.IntegrationTests.Constants;
using api.IntegrationTests.ControllersTests.ControllersFixtures;
using api.IntegrationTests.WebAppFactories;

namespace api.IntegrationTests.ControllersTests;

[Collection(CollectionConstants.IntegrationTestsDatabase)]
public class StoryControllerTests : IClassFixture<StoryControllerFixture>
{
    private readonly CustomWebAppFactory _factory;

    public StoryControllerTests(StoryControllerFixture fixture)
    {
        _factory = fixture.Factory;
    }
    
    [Theory]
    [InlineData(46, 15, false)]
    [InlineData(5, 5, true)]
    [InlineData(15, 15, true)]
    [InlineData(-15, 0, false)]
    [InlineData(0, 0, false)]
    [InlineData(1, 1, true)]
    [InlineData(null, 15, false)]
    public async Task GetAllStories_GivenValidLastId_ReturnExpectedResponse(int? lastId, int expectedStoriesCount, bool containsStoryWithId1 = true)
    {
        // Arrange
        HttpClient client = _factory.CreateClient();
        
        // Act
        HttpResponseMessage response = await client.GetAsync($"/collab-stories?lastId={lastId}");
        
        // Assert
        MediaTypeHeaderValue? contentType = response.Content.Headers.ContentType;
        
        response.EnsureSuccessStatusCode();
        Assert.NotNull(contentType);
        Assert.Equal(MediaTypeNames.Application.Json, contentType.MediaType);
        Assert.Equal("utf-8", contentType.CharSet);
        
        var pagedStories = await response.Content.ReadFromJsonAsync<PagedKeysetStoryList<StoryMainInfoDto>>();
        Assert.NotNull(pagedStories);
        Assert.Equal(expectedStoriesCount, pagedStories.Items.Count);
        Assert.Equal(containsStoryWithId1, pagedStories.Items.Any(s => s.Id == 1));
    }
    
    // Query parameters that don't exist should be equivalent to no additional query param
    // As such, they are valid
    [Theory]
    [InlineData("test=abc")]
    [InlineData("lastId=")]
    [InlineData("id=30")]
    [InlineData("")]
    public async Task GetAllStories_GivenNonExistingParams_ReturnSameAsNoParams(string queryString)
    {
        // Arrange
        HttpClient client = _factory.CreateClient();
        
        // Act
        HttpResponseMessage actualResponse = await client.GetAsync($"/collab-stories?{queryString}");
        HttpResponseMessage expectedResponse = await client.GetAsync("/collab-stories");
        
        // Assert
        MediaTypeHeaderValue? actualContentType = actualResponse.Content.Headers.ContentType;
        
        actualResponse.EnsureSuccessStatusCode();
        Assert.NotNull(actualContentType);
        Assert.Equal(MediaTypeNames.Application.Json, actualContentType.MediaType);
        Assert.Equal("utf-8", actualContentType.CharSet);
        Assert.Equal(expectedResponse.StatusCode, actualResponse.StatusCode);
        Assert.Equal(expectedResponse.Content.Headers.ContentType, actualResponse.Content.Headers.ContentType);
        
        var actualPagedStories = await actualResponse.Content.ReadFromJsonAsync<PagedKeysetStoryList<StoryMainInfoDto>>();
        var expectedPagedStories = await expectedResponse.Content.ReadFromJsonAsync<PagedKeysetStoryList<StoryMainInfoDto>>();
        Assert.NotNull(actualPagedStories);
        Assert.NotNull(expectedPagedStories);
        Assert.Equal(expectedPagedStories.Items, actualPagedStories.Items);
    }
    
    [Theory]
    [InlineData("lastId=abc")]
    [InlineData("lastId=null")]
    public async Task GetAllStories_GivenInvalidQueryString_ReturnBadRequest(string invalidQueryString)
    {
        // Arrange
        HttpClient client = _factory.CreateClient();
        
        // Act
        HttpResponseMessage response = await client.GetAsync($"/collab-stories?{invalidQueryString}");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);;
    }
}