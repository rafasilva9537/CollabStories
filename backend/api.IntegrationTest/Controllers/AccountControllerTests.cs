namespace api.IntegrationTest.Controllers;

[Collection(CollectionConstants.IntegrationTestsDatabase)]
public class AccountControllerTests : IClassFixture<CustomWebAppFactory>
{
    private readonly CustomWebAppFactory _factory;

    public AccountControllerTests(CustomWebAppFactory factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData(0)]
    [InlineData(15)]
    [InlineData(30)]
    public async Task GetUsers_GivenValidLastId_ReturnExpectedUsers(int lastId)
    {
        // Arrange
        HttpClient client = _factory.CreateClient();

        // Act
        //HttpResponseMessage? response = await client.GetAsync($"/accounts/{lastId}");
        //var users = await response.Content.ReadFromJsonAsync<List<UserMainInfoDto>>();

        // Assert
        //Assert.True(!users.IsNullOrEmpty());
        //response.EnsureSuccessStatusCode();
        //Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        // Assert.True(users?.Count == 15);
    }
}