namespace api.IntegrationTest.Controllers;

[Collection(CollectionConstants.IntegrationTestsDatabase)]
public class StoryControllerTests : IClassFixture<CustomWebAppFactory>
{
    private readonly CustomWebAppFactory _factory;

    public StoryControllerTests(CustomWebAppFactory factory)
    {
        _factory = factory;
    }
}