using api.Constants;
using api.Models;
using Bogus;

namespace api.IntegrationTest.Services.Data;

public class FakeDataGenerator
{
    private Faker<AppUser> _fakeAppUser;
    private Faker<Story> _fakeStory;
    private Faker<AuthorInStory> _fakeAuthorInStory;
    private Faker<StoryPart> _fakeStoryPart;

    public FakeDataGenerator()
    {
        Randomizer.Seed = new Random(10);

        DateTimeOffset userStartDate = new(2022, 1, 1, 0, 0, 0, new TimeSpan(0));
        DateTimeOffset userEndDate = userStartDate.AddYears(10);
        _fakeAppUser = new Faker<AppUser>()
            .RuleFor(au => au.Nickname, f => f.Name.FirstName())
            .RuleFor(au => au.UserName, (f, au) => f.Internet.UserName(au.Nickname))
            .RuleFor(au => au.Description, f => f.Lorem.Sentence(5, 15))
            .RuleFor(au => au.Email, (f, au) => f.Internet.Email(au.UserName))
            .RuleFor(au => au.CreatedDate, f => f.Date.BetweenOffset(userStartDate, userEndDate))
            .RuleFor(
                au => au.ProfileImage, f => Path.Combine(
                    DirectoryPathConstants.Media,
                    DirectoryPathConstants.Images,
                    DirectoryPathConstants.ProfileImages,
                    f.Random.Guid().ToString()
                )
            );

        DateTimeOffset storyStartDate = userStartDate.AddHours(1);
        DateTimeOffset storyEndDate = userEndDate.AddDays(40);
        _fakeStory = new Faker<Story>()
            .RuleFor(s => s.Title, f => f.Lorem.Sentence(2, 6))
            .RuleFor(s => s.Description, f => f.Lorem.Sentence(8, 12))
            .RuleFor(s => s.MaximumAuthors, f => f.Random.Int(1, 16))
            .RuleFor(s => s.TurnDurationSeconds, f => f.Random.Int(30, 3600))
            .RuleFor(s => s.CreatedDate, f => f.Date.BetweenOffset(storyStartDate, storyEndDate))
            .RuleFor(s => s.UpdatedDate, (f, s) => f.Date.BetweenOffset(s.CreatedDate, storyEndDate));

        _fakeStoryPart = new Faker<StoryPart>()
            .RuleFor(sp => sp.Text, f => f.Lorem.Sentence(3, 100))
            .RuleFor(sp => sp.CreatedDate, f => f.Date.BetweenOffset(storyStartDate, storyEndDate));

        _fakeAuthorInStory = new Faker<AuthorInStory>()
            .RuleFor(ais => ais.EntryDate, (f, ais) => f.Date.BetweenOffset(storyStartDate, storyEndDate));
    }

    public List<AppUser> GenerateAppUsers(int quantity)
    {
        List<AppUser> appUsers = _fakeAppUser.GenerateForever().Take(quantity).ToList();
        return appUsers;
    }

    public List<Story> GenerateStories(int quantity, int? userId = null)
    {
        Faker<Story> extendedFakeStory = _fakeStory
            .CustomInstantiator(f => new Story { UserId = userId });

        List<Story> stories = extendedFakeStory.GenerateForever().Take(quantity).ToList();
        return stories;
    }

    public List<StoryPart> GenerateStoryParts(int quantity, int userRangeId)
    {
        Faker<StoryPart> extendedFakeStoryPart = _fakeStoryPart
            .CustomInstantiator(f => new StoryPart { UserId = f.Random.Int(1, userRangeId) });

        List<StoryPart> storyParts = extendedFakeStoryPart.GenerateForever().Take(quantity).ToList();
        return storyParts;
    }

    public List<Story> GenerateStoriesWithStoryParts(int quantity, int? userId = null)
    {
        Faker<Story> extendedFakeStory = _fakeStory
            .RuleFor(s => s.StoryParts, (f, s) =>
            {
                foreach (StoryPart storyPart in GenerateStoryParts(20, s.Id))
                {
                    s.StoryParts.Add(storyPart);
                }
                return s.StoryParts;
            });

        List<Story> stories = extendedFakeStory.GenerateForever().Take(quantity).ToList();
        return stories;
    }

    public List<AppUser> GenerateAppUsersWithAllRelatedTables(int quantity)
    {
        var storyPart = GenerateStoryParts(2, 3);

        Faker<AppUser> extendedFakeAppUser = _fakeAppUser
            .RuleFor(au => au.Stories, (f, au) =>
            {
                foreach (Story story in GenerateStoriesWithStoryParts(f.Random.Int(1, 50), au.Id))
                {
                    au.Stories.Add(story);
                }
                return au.Stories;
            });

        List<AppUser> users = extendedFakeAppUser.GenerateForever().Take(quantity).ToList();
        return users;
    }
}
