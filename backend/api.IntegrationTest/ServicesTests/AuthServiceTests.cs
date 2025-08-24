using api.Constants;
using api.Data;
using api.Dtos.AppUser;
using api.Exceptions;
using api.IntegrationTests.Constants;
using api.IntegrationTests.WebAppFactories;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;

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
        DateTimeOffset fixedDate = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        dateTimeProvider.UtcNow.Returns(fixedDate);
        var customFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(IDateTimeProvider));
                services.AddTransient<IDateTimeProvider>(_ => dateTimeProvider);
            });
        });
        
        using IServiceScope scope = customFactory.Services.CreateScope();
        IAuthService authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        
        (expectedUserName, expectedEmail) = UniqueDataCreation.CreateUniqueUserNameAndEmail(expectedUserName, expectedEmail); // Avoid duplicated values
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
        Assert.Equal(dateTimeProvider.UtcNow, createdUser.CreatedDate);
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
        
        (userName, email) = UniqueDataCreation.CreateUniqueUserNameAndEmail(userName, email);
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
        
        (userName, email) = UniqueDataCreation.CreateUniqueUserNameAndEmail(userName, email);
        AppUser newUser = new() { UserName = userName, Email = email};
        await userManager.CreateAsync(newUser, password);
        
        LoginUserDto loginUserDto = new() { UserName = userName, Password = password };
        
        // Act
        string? loginToken = await authService.LoginAsync(loginUserDto);
        
        // Assert
        Assert.False(string.IsNullOrWhiteSpace(loginToken));
    }
    
    [Theory]
    [InlineData("test_user", "test.user@example.com", "TestPassw0rd", "TestPassw0rd123")]
    public async Task LoginAsync_WithInvalidPassword_ReturnsNull(string userName, string email, string originalPassword, string invalidPassword)
    {
        // Arrange
        using IServiceScope scope = _factory.Services.CreateScope();
        IAuthService authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        
        (userName, email) = UniqueDataCreation.CreateUniqueUserNameAndEmail(userName, email);
        AppUser newUser = new() { UserName = userName, Email = email};
        await userManager.CreateAsync(newUser, originalPassword);
        
        LoginUserDto loginUserDto = new() { UserName = userName, Password = invalidPassword };
        
        // Act
        string? loginToken = await authService.LoginAsync(loginUserDto);
        
        // Assert
        Assert.Null(loginToken);
    }
    
    [Theory]
    [InlineData("test_user", "test.user@example.com")]
    [InlineData("", "test.user@example.com")]
    public async Task LoginAsync_WithNonExistingUser_ThrowsUserNotFoundException(string userName, string password)
    {
        // Arrange
        using IServiceScope scope = _factory.Services.CreateScope();
        IAuthService authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        
        string suffix = Guid.NewGuid().ToString();
        userName = $"{userName}_{suffix}";
        
        LoginUserDto loginUserDto = new() { UserName = userName, Password = password };
        
        // Act/Assert
        await Assert.ThrowsAsync<UserNotFoundException>(() => authService.LoginAsync(loginUserDto));
    }

    [Theory]
    [InlineData("test_user_1", "test.user1@example.com")]
    [InlineData("test_user_2", "test.user2@example.com")]
    [InlineData("test_user_3", "test.user3@example.com")]
    public async Task GetUserAsync_WhenExists_ReturnsExpectedUser(string baseUserName, string baseEmail)
    {
        // Arrange
        using IServiceScope scope = _factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        IAuthService authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        
        AppUser expectedUser = UniqueDataCreation.CreateUniqueTestUser(baseUserName, baseEmail);
        await dbContext.Users.AddAsync(expectedUser);
        await dbContext.SaveChangesAsync();
        
        // Act
        AppUserDto? actualUserDto = await authService.GetUserAsync(expectedUser.UserName);
        
        // Assert
        Assert.NotNull(actualUserDto);
        Assert.Equal(expectedUser.UserName, actualUserDto.UserName);
        Assert.Equal(expectedUser.Email, actualUserDto.Email);
        Assert.Equal(string.Empty, actualUserDto.Nickname);
    }
    
    [Theory]
    [InlineData("test_user_1", "test.user1@example.com")]
    [InlineData("test_user_2", "test.user2@example.com")]
    [InlineData("test_user_3", "test.user3@example.com")]
    public async Task GetUserAsync_WhenDoesNotExists_ReturnsNull(string baseUserName, string baseEmail)
    {
        // Arrange
        using IServiceScope scope = _factory.Services.CreateScope();
        IAuthService authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        
        (string nonExistingUserName, _) = UniqueDataCreation.CreateUniqueUserNameAndEmail(baseUserName, baseEmail);
        
        // Act
        AppUserDto? actualUserDto = await authService.GetUserAsync(nonExistingUserName);
        
        // Assert
        Assert.Null(actualUserDto);
    }
}