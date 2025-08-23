using api.Constants;
using api.Dtos.AppUser;
using api.Exceptions;
using api.IntegrationTests.Constants;
using api.IntegrationTests.WebAppFactories;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace api.IntegrationTests.ServicesTests;

[Collection(CollectionConstants.IntegrationTestsDatabase)]
public class AuthServiceTests : IClassFixture<CustomWebAppFactory>
{
    private readonly CustomWebAppFactory _factory;
    
    public AuthServiceTests(CustomWebAppFactory factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("testUser", "test-user@gmail.com", "This-is-46-test-pasSword")]
    [InlineData("testUser", "test-user@gmail.com", "passwordTest123")]
    public async Task RegisterAsync_WithValidFields_CreateUserAndReturnsExpectedToken(string expectedUserName, string expectedEmail, string expectedPassword)
    {
        // Arrange
        using IServiceScope scope = _factory.Services.CreateScope();
        IAuthService authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        string suffix = Guid.NewGuid().ToString(); // Avoid duplicated values
        expectedUserName = $"{expectedUserName}-{suffix}";
        expectedEmail = $"{expectedEmail}-{suffix}";
        
        RegisterUserDto registerUserDto = new()
        {
            UserName = expectedUserName,
            Email = expectedEmail,
            Password = expectedPassword,
        };
        
        // Act
        string token = await authService.RegisterAsync(registerUserDto);

        // Assert
        Assert.False(string.IsNullOrEmpty(token));
        
        AppUser? createdUser = await userManager.Users.SingleOrDefaultAsync(u => u.UserName == expectedUserName);
        Assert.NotNull(createdUser);
        
        bool hasUserRole = await userManager.IsInRoleAsync(createdUser, RoleConstants.User);
        Assert.False(string.IsNullOrEmpty(createdUser.PasswordHash));
        Assert.Equal(expectedUserName, createdUser.UserName);
        Assert.Equal(expectedEmail, createdUser.Email);
        Assert.Equal(string.Empty, createdUser.Nickname);
        Assert.True(hasUserRole);
    }
    
    [Theory]
    [InlineData("testUser+Name3", "test3-user@gmailcom", "this-is-46-test-password")]
    [InlineData("", "test4-user@gmailcom", "This-is-46-test-pasSword")]
    [InlineData("test@User", "test5-user@gmailcom", "this-is-46-test-pasSword")]
    public async Task RegisterAsync_WithInvalidUserName_ThrowsException(string userName, string email, string password)
    {
        // Arrange
        using IServiceScope scope = _factory.Services.CreateScope();
        IAuthService authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        
        RegisterUserDto registerUserDto = new()
        {
            UserName = userName,
            Email = email,
            Password = password,
        };
        
        // Act/Assert
        await Assert.ThrowsAsync<UserRegistrationException>(() => authService.RegisterAsync(registerUserDto));
    }
    
    [Theory]
    [InlineData("test_user", "test6-user@gmailcom", "")]
    [InlineData("test_user", "test7-user@gmailcom", "1234")]
    [InlineData("test_user", "test8-user@gmailcom", "12345678")]
    [InlineData("test_user", "test9-user@gmailcom", "123TEst")]
    public async Task RegisterAsync_WithInvalidUserPassword_ThrowsException(string userName, string email, string password)
    {
        // Arrange
        using IServiceScope scope = _factory.Services.CreateScope();
        IAuthService authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        
        string suffix = Guid.NewGuid().ToString();
        userName = $"{userName}-{suffix}";
        email = $"{email}-{suffix}";
        
        RegisterUserDto registerUserDto = new()
        {
            UserName = userName,
            Email = email,
            Password = password,
        };
        
        // Act/Assert
        await Assert.ThrowsAsync<UserRegistrationException>(() => authService.RegisterAsync(registerUserDto));
    }
    
    [Theory]
    [InlineData("test_user_1", "test.user1@example.com", "TestPassw0rd!")]
    [InlineData("test_user_2", "test.user2@example.com", "SecureP@ss123")]
    [InlineData("test_user_3", "test.user3@example.com", "StrongP@ssw0rd")]
    public async Task LoginAsync_WithValidCredentials_ReturnsToken(string userName, string email, string password)
    {
        // Arrange
        using IServiceScope scope = _factory.Services.CreateScope();
        IAuthService authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        
        string suffix = Guid.NewGuid().ToString();
        userName = $"{userName}_{suffix}";
        email = $"{email}_{suffix}";

        AppUser newUser = new() { UserName = userName, Email = email};
        await userManager.CreateAsync(newUser, password);
        
        LoginUserDto loginUserDto = new() { UserName = userName, Password = password };
        
        // Act
        string? loginToken = await authService.LoginAsync(loginUserDto);
        
        // Assert
        Assert.False(string.IsNullOrWhiteSpace(loginToken));
    }
}