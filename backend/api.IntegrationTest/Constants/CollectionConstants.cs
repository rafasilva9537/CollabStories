namespace api.IntegrationTests.Constants;

public static class CollectionConstants
{
    // Collection needed to take care of conflicts related to database being created/deleted in parallel
    // In Services with TestDatabaseFixture and in Controllers with CustomWebAppFactory (both manipulate the database)
    public const string IntegrationTestsDatabase = "IntegrationTestsDatabase";
}
