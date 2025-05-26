using System.Net.Http.Json;
using api.Dtos.AppUser;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace api.IntegrationTest.Controllers;

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
        HttpResponseMessage? response = await client.GetAsync($"accounts/{lastId}");
        List<UserMainInfoDto>? users = await response.Content.ReadFromJsonAsync<List<UserMainInfoDto>>();

        // Act

        // Assert
        Assert.True(!users.IsNullOrEmpty());
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        Assert.True(users?.Count == 15);
    }
}