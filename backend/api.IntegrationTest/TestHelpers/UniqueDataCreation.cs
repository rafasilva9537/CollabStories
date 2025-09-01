using api.Models;

namespace api.IntegrationTests.TestHelpers;

public static class UniqueDataCreation
{
    /// Generates a unique username and email by appending a GUID-based suffix to the provided base values.
    /// <param name="baseUserName">The base username to be transformed into a unique username.</param>
    /// <param name="baseEmail">The base email to be transformed into a unique email.</param>
    /// <returns>A tuple containing the unique username and unique email.</returns>
    public static (string, string) CreateUniqueUserNameAndEmail(string baseUserName, string baseEmail)
    {
        string suffix = Guid.NewGuid().ToString();
        baseUserName = $"{baseUserName}_{suffix}";
        baseEmail = baseEmail.Replace("@", $"{suffix}@");
        
        return (baseUserName, baseEmail);
    }

    /// Creates a unique test user by generating a unique username and email based on the provided base values.
    /// <param name="baseUserName">The base username to be used as a template for the unique username.</param>
    /// <param name="baseEmail">The base email to be used as a template for the unique email.</param>
    /// <returns>An AppUser object with a unique username and email set, along with normalized values.</returns>
    public static AppUser CreateUniqueTestUser(string baseUserName, string baseEmail)
    {
        (string uniqueUserName, string uniqueEmail) = CreateUniqueUserNameAndEmail(baseUserName, baseEmail);
        return new AppUser()
        {
            UserName = uniqueUserName,
            Email = uniqueEmail,
            NormalizedUserName = uniqueUserName.ToUpper(),
            NormalizedEmail = uniqueEmail.ToUpper(),
        };
    }
}