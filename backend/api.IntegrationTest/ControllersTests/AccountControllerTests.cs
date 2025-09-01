using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using api.Constants;
using api.Data;
using api.Dtos.AppUser;
using api.Dtos.Pagination;
using api.IntegrationTests.Constants;
using api.IntegrationTests.ControllersTests.ControllersFixtures;
using api.IntegrationTests.TestHelpers;
using api.IntegrationTests.WebAppFactories;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace api.IntegrationTests.ControllersTests;

[Collection(CollectionConstants.IntegrationTestsDatabase)]
public class AccountControllerTests : IClassFixture<AccountControllerFixture>
{
    private readonly AuthHandlerWebAppFactory _factory;
    private readonly AppUser _defaultUser;
    
    public AccountControllerTests(AccountControllerFixture fixture)
    {
        _factory = fixture.Factory;
        _defaultUser = fixture.DefaultUser;
    }
    
    [Fact]
    public async Task GetUser_WhenAuthenticated_ReturnsExpectedUser()
    {
        // Arrange
        AppUser expectedUser = _defaultUser;
        HttpClient client = _factory.CreateClientWithAuth(expectedUser, RoleConstants.User);
        
        // Act
        HttpResponseMessage response = await client.GetAsync($"/accounts/{expectedUser.UserName}");
        
        // Assert
        MediaTypeHeaderValue? contentType = response.Content.Headers.ContentType;
        
        response.EnsureSuccessStatusCode();
        Assert.NotNull(contentType);
        Assert.Equal(MediaTypeNames.Application.Json, contentType.MediaType);
        Assert.Equal("utf-8", contentType.CharSet);

        AppUserDto? user = await response.Content.ReadFromJsonAsync<AppUserDto>();
        Assert.NotNull(user);
        Assert.Equal(expectedUser.UserName, user.UserName);
        Assert.Equal(expectedUser.Email, user.Email);
    }
    
    [Fact]
    public async Task GetUsers_WithKeysetPagination_ReturnsCorrectlyPagedUsers()
    {
        // Arrange
        const int pageSize = 15;
        const int newUsersCount = 19;
        DateTimeOffset baseDate = new(2025, 9, 1, 12, 0, 0, TimeSpan.Zero);

        using IServiceScope scope = _factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.AppUser
            .Where(u => u.Id != _defaultUser.Id)
            .ExecuteDeleteAsync();

        List<AppUser> newUsers = [];
        for (int i = 0; i < newUsersCount; i++)
        {
            newUsers.Add(TestModelFactory.CreateAppUserModel(createdDate: baseDate.AddDays(i)));
        }
        TestDataSeeder.SeedMultipleUsers(dbContext, newUsers);
        
        List<AppUser> totalUsers = dbContext.AppUser.ToList();
        var expectedOrderedUsers = totalUsers
            .OrderByDescending(u => u.CreatedDate)
            .ThenBy(u => u.UserName)
            .ToList();

        HttpClient client = _factory.CreateClient();

        // Act: First page
        HttpResponseMessage responsePage1 = await client.GetAsync("/accounts");
        responsePage1.EnsureSuccessStatusCode();
        var usersPage1 = await responsePage1.Content.ReadFromJsonAsync<PagedKeysetUserList<UserMainInfoDto>>();

        // Assert: First page
        Assert.NotNull(usersPage1);
        Assert.Equal(pageSize, usersPage1.Items.Count);
        Assert.True(usersPage1.HasMore);
        
        Assert.Equal(
            expectedOrderedUsers.Take(pageSize).Select(u => u.UserName),
            usersPage1.Items.Select(u => u.UserName)
        );

        AppUser lastUserOnPage1 = expectedOrderedUsers[pageSize];
        Assert.NotNull(usersPage1.NextDate);
        
        Assert.NotNull(usersPage1.NextUserName);;
        Assert.Equal(lastUserOnPage1.CreatedDate, usersPage1.NextDate.Value);
        Assert.Equal(lastUserOnPage1.UserName, usersPage1.NextUserName);

        // Act: Second page
        string nextDate = Uri.EscapeDataString(usersPage1.NextDate.Value.ToString("o"));
        string nextUserName = Uri.EscapeDataString(usersPage1.NextUserName);
        HttpResponseMessage responsePage2 = await client.GetAsync($"/accounts?lastDate={nextDate}&lastUserName={nextUserName}");
        var usersPage2 = await responsePage2.Content.ReadFromJsonAsync<PagedKeysetUserList<UserMainInfoDto>>();

        // Assert: Second page
        responsePage2.EnsureSuccessStatusCode();
        Assert.NotNull(usersPage2);
        Assert.Equal(totalUsers.Count - pageSize, usersPage2.Items.Count);
        Assert.Equal(
            expectedOrderedUsers.Skip(pageSize).Select(u => u.UserName),
            usersPage2.Items.Select(u => u.UserName)
        );

        // No more pages
        Assert.False(usersPage2.HasMore);
        Assert.Null(usersPage2.NextDate);
        Assert.Null(usersPage2.NextUserName);
    }
}