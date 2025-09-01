using api.Models;

namespace api.IntegrationTests.TestHelpers;

// Navigation properties shouldn't be used here. Avoid conflicts with DbContext on test data seed.
internal static class TestModelFactory
{
    /// <summary>
    /// Creates and returns a new instance of the Story model with specified or default properties.
    /// </summary>
    /// <returns>A new instance of the Story model with specified or default properties.</returns>
    public static Story CreateStoryModel(
        string title = "Test Story",
        string description = "This is a test story",
        int maximumAuthors = 5,
        int turnDurationSeconds = 300,
        DateTimeOffset? createdDate = null,
        DateTimeOffset? updatedDate = null,
        bool isFinished = false,
        int? userId = null,
        int? currentAuthorId = null)
    {
        if (createdDate is null && updatedDate is null)
        {
            createdDate = DateTimeOffset.UtcNow;
            updatedDate = createdDate;
        }
        else if (createdDate is not null && updatedDate is null)
        {
            updatedDate = createdDate;
        }
        
        Story newStory = new()
        {
            Title = title,
            Description = description,
            MaximumAuthors = maximumAuthors,
            TurnDurationSeconds = turnDurationSeconds,
            CreatedDate = createdDate ?? DateTimeOffset.UtcNow,
            UpdatedDate = updatedDate ?? DateTimeOffset.UtcNow,
            IsFinished = isFinished,
            UserId = userId,
            CurrentAuthorId = currentAuthorId,
        };

        return newStory;
    }

    /// <summary>
    /// Creates and returns a new instance of the AppUser model with specified or default properties.
    /// It's unique by default, using a GUID-based suffix on username and email.
    /// </summary>
    /// <param name="baseUserName">The base username to be used.</param>
    /// <param name="baseEmail">The base email to be used.</param>
    /// <param name="nickname">The nickname of the user.</param>
    /// <param name="description">A description of the user.</param>
    /// <param name="profileImage">The profile image URL of the user.</param>
    /// <param name="createdDate">The creation date of the user. Defaults to the current utc DateTimeOffset if null.</param>
    /// <param name="isUnique">Indicates whether the username and email should be unique. If true, unique values will have an appended GUID-based suffix.</param>
    /// <returns>A new instance of the AppUser model with the specified or default properties.</returns>
    public static AppUser CreateAppUserModel(
        string baseUserName = "test_user",
        string baseEmail = "testuser@example.com",
        string nickname = "TestUser",
        string description = "This is a test user",
        string profileImage = "",
        DateTimeOffset? createdDate = null,
        bool isUnique = true)
    {
        (string userName, string email) = (baseUserName, baseEmail);
        if (isUnique)
        {
            (userName, email) = UniqueDataCreation.CreateUniqueUserNameAndEmail(baseUserName, baseEmail);
        }

        if(createdDate is null) createdDate = DateTimeOffset.UtcNow;
        
        // Normalized properties are needed because UserManger doesn't work properly without it
        AppUser newUser = new()
        {
            UserName = userName,
            Email = email,
            NormalizedUserName = userName.ToUpper(),
            NormalizedEmail = email.ToUpper(),
            Nickname = nickname,
            Description = description,
            ProfileImage = profileImage,
            CreatedDate = createdDate.Value,
        };

        return newUser;
    }
    
    
    
    /// <summary>
    /// Creates and returns a list of unique AppUser models with specified count.
    /// </summary>
    /// <param name="count">Number of users to create.</param>
    /// <param name="baseUserName">Base username template.</param>
    /// <param name="baseEmail">Base email template.</param>
    /// <returns>List of unique AppUser models.</returns>
    public static List<AppUser> CreateMultipleAppUserModels(
        int count,
        string baseUserName = "test_user",
        string baseEmail = "testuser@example.com")
    {
        return Enumerable.Range(1, count)
            .Select(_ => CreateAppUserModel(baseUserName, baseEmail))
            .ToList();
    }

    public static AuthorInStory CreateAuthorInStory(
        int storyId,
        int userId,
        DateTimeOffset? entryDate = null)
    {
        entryDate ??= DateTimeOffset.UtcNow;

        AuthorInStory newAuthorInStory = new()
        {
            StoryId = storyId,
            AuthorId = userId,
            EntryDate = entryDate.Value,
        };

        return newAuthorInStory;
    }
}