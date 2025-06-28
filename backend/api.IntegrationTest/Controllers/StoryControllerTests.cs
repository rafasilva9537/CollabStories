namespace api.IntegrationTests.Controllers;

[Collection(CollectionConstants.IntegrationTestsDatabase)]
public class StoryControllerTests : IClassFixture<CustomWebAppFactory>
{
    private readonly CustomWebAppFactory _factory;

    public StoryControllerTests(CustomWebAppFactory factory)
    {
        _factory = factory;
    }
}