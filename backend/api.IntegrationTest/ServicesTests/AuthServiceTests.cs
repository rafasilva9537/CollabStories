using api.IntegrationTests.WebAppFactories;

namespace api.IntegrationTests.ServicesTests;

public class AuthServiceTests : IClassFixture<AuthHandlerWebAppFactory>
{
    private readonly AuthHandlerWebAppFactory _factory;
    
    public AuthServiceTests(AuthHandlerWebAppFactory factory)
    {
        _factory = factory;
    }
}