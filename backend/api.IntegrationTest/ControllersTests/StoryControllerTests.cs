using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using api.Dtos.Story;
using api.IntegrationTests.Constants;
using api.IntegrationTests.WebAppFactories;

namespace api.IntegrationTests.ControllersTests;

[Collection(CollectionConstants.IntegrationTestsDatabase)]
public class StoryControllerTests : IClassFixture<CustomWebAppFactory>
{
    private readonly CustomWebAppFactory _factory;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public StoryControllerTests(CustomWebAppFactory factory)
    {
        _factory = factory;
        
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };
    }
    
    [Theory]
    [InlineData(100, 15, false)]
    [InlineData(1000, 15, false)]
    [InlineData(45, 15, false)]
    [InlineData(5, 4, true)]
    [InlineData(16, 15, true)]
    [InlineData(-15, 0, false)]
    [InlineData(0, 0, false)]
    [InlineData(null, 15, false)]
    public async Task GetAllStories_GivenValidLastId_ReturnExpectedStories(int? lastId, int expectedStoriesCount, bool containsStoryWithId1 = true)
    {
        // Arrange
        HttpClient client = _factory.CreateClient();
        
        // Act
        HttpResponseMessage response = await client.GetAsync($"/collab-stories?lastId={lastId}");
        
        // Assert
        MediaTypeHeaderValue? contentType = response.Content.Headers.ContentType;
        
        response.EnsureSuccessStatusCode();
        Assert.NotNull(contentType);
        Assert.Equal("application/json", contentType.MediaType);
        Assert.Equal("utf-8", contentType.CharSet);

        string? jsonResponse = await response.Content.ReadAsStringAsync();
        var stories = JsonSerializer.Deserialize<IList<StoryMainInfoDto>>(jsonResponse, _jsonSerializerOptions);
        Assert.NotNull(stories);
        Assert.Equal(expectedStoriesCount, stories.Count);
        
        Assert.Equal(containsStoryWithId1, stories.Any(s => s.Id == 1));
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
        Assert.Equal("application/json", actualContentType.MediaType);
        Assert.Equal("utf-8", actualContentType.CharSet);
        Assert.Equal(expectedResponse.StatusCode, actualResponse.StatusCode);
        Assert.Equal(expectedResponse.Content.Headers.ContentType, actualResponse.Content.Headers.ContentType);

        string jsonActualResponse = await actualResponse.Content.ReadAsStringAsync();
        string jsonExpectedResponse = await expectedResponse.Content.ReadAsStringAsync();
        var actualStories = JsonSerializer.Deserialize<IList<StoryMainInfoDto>>(jsonActualResponse, _jsonSerializerOptions);
        var expectedStories = JsonSerializer.Deserialize<IList<StoryMainInfoDto>>(jsonExpectedResponse, _jsonSerializerOptions);
        Assert.Equal(expectedStories, actualStories);
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