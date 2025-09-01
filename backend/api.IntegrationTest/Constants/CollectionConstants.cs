namespace api.IntegrationTests.Constants;

public static class CollectionConstants
{
    // Collection needed to take care of conflicts related to database being created/deleted in parallel
    // Temporary solution until Test containers are added.
    public const string IntegrationTestsDatabase = "IntegrationTestsDatabase";
}
