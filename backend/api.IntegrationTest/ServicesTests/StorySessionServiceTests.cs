using api.IntegrationTests.Constants;
using api.IntegrationTests.WebAppFactories;

namespace api.IntegrationTests.ServicesTests;

[Collection(CollectionConstants.IntegrationTestsDatabase)]
public class StorySessionServiceTests : IClassFixture<CustomWebAppFactory>
{
    private readonly CustomWebAppFactory _factory;

    public StorySessionServiceTests(CustomWebAppFactory factory)
    {
        _factory = factory;
    }
}