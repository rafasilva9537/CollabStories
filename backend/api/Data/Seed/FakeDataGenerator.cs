using api.Constants;
using api.Models;
using Bogus;
using Microsoft.IdentityModel.Tokens;

namespace api.Services.Data;

public class FakeDataGenerator
{
    private readonly DateTimeOffset _userStartDate;
    private readonly DateTimeOffset _userEndDate;
    private readonly DateTimeOffset _storyStartDate;
    private readonly DateTimeOffset _storyEndDate;

    public FakeDataGenerator()
    {
        Randomizer.Seed = new Random(10);

        _userStartDate = new(2022, 1, 1, 0, 0, 0, new TimeSpan(0));
        _userEndDate = _userStartDate.AddYears(10);

        _storyStartDate = _userStartDate.AddHours(1);
        _storyEndDate = _userEndDate.AddDays(40);
    }

    public List<AppUser> GenerateAppUsers(int quantity)
    {
        Faker<AppUser> fakeAppUser = new Faker<AppUser>()
            .RuleFor(au => au.Nickname, f => f.Name.FirstName())
            .RuleFor(au => au.UserName, (f, au) => f.Internet.UserName(au.Nickname))
            .RuleFor(au => au.Description, f => f.Lorem.Sentence(5, 15))
            .RuleFor(au => au.Email, (f, au) => f.Internet.Email(au.UserName))
            .RuleFor(au => au.CreatedDate, f => f.Date.BetweenOffset(_userStartDate, _userEndDate))
            .RuleFor(
                au => au.ProfileImage, f => Path.Combine(
                    DirectoryPathConstants.Media,
                    DirectoryPathConstants.Images,
                    DirectoryPathConstants.ProfileImages,
                    f.Random.Guid().ToString()
                )
            );

        List<AppUser> appUsers = fakeAppUser.GenerateForever().Take(quantity).ToList();
        return appUsers;
    }

    public List<Story> GenerateStories(int quantity, List<AppUser>? possibleUsers = null)
    {
        Faker<Story> fakeStory = new Faker<Story>()
            .RuleFor(s => s.Title, f => f.Lorem.Sentence(2, 6))
            .RuleFor(s => s.Description, f => f.Lorem.Sentence(8, 12))
            .RuleFor(s => s.MaximumAuthors, f => f.Random.Int(2, 16))
            .RuleFor(s => s.TurnDurationSeconds, f => f.Random.Int(30, 3600))
            .RuleFor(s => s.CreatedDate, f => f.Date.BetweenOffset(_storyStartDate, _storyEndDate))
            .RuleFor(s => s.UpdatedDate, (f, s) => f.Date.BetweenOffset(s.CreatedDate, _storyEndDate))
            .RuleFor(s => s.UserId, f =>
            {
                if (possibleUsers.IsNullOrEmpty()) return null;
                AppUser randomUser = f.PickRandom(possibleUsers);
                return randomUser.Id;
            });

        if (!possibleUsers.IsNullOrEmpty())
        {
            fakeStory = fakeStory.RuleFor(s => s.AuthorInStory, (f, s) =>
            {
                List<AuthorInStory> newAuthors = GenerateAuthorsInStory(f.Random.Int(1, 7), s.Id, possibleUsers!)
                    .DistinctBy(ais => (ais.AuthorId, ais.StoryId))
                    .ToList();

                foreach (var newAuthor in newAuthors)
                {
                    s.AuthorInStory.Add(newAuthor);
                }

                return s.AuthorInStory;
            });
        }

        List<Story> stories = fakeStory.GenerateForever().Take(quantity).ToList();
        return stories;
    }

    public List<StoryPart> GenerateStoryParts(int quantity, List<Story> possibleStories, List<AppUser>? possibleUsers = null)
    {
        Faker<StoryPart> fakeStoryPart = new Faker<StoryPart>()
            .RuleFor(sp => sp.Text, f => f.Lorem.Sentence(3, 100))
            .RuleFor(sp => sp.CreatedDate, f => f.Date.BetweenOffset(_storyStartDate, _storyEndDate))
            .RuleFor(sp => sp.StoryId, f =>
            {
                Story randomStory = f.PickRandom(possibleStories);
                return randomStory.Id;
            })
            .RuleFor(sp => sp.UserId, f =>
            {
                if (possibleUsers.IsNullOrEmpty()) return null;
                AppUser randomUser = f.PickRandom(possibleUsers);
                return randomUser.Id;
            });

        List<StoryPart> storyParts = fakeStoryPart.GenerateForever().Take(quantity).ToList();
        return storyParts;
    }

    public List<AuthorInStory> GenerateAuthorsInStory(int quantity, int storyId, List<AppUser> possibleAuthors)
    {
        Faker<AuthorInStory> fakeAuthorInStory = new Faker<AuthorInStory>()
            .RuleFor(ais => ais.EntryDate, (f, ais) => f.Date.BetweenOffset(_storyStartDate, _storyEndDate))
            .RuleFor(ais => ais.StoryId, f => storyId)
            .RuleFor(ais => ais.AuthorId, f =>
            {
                AppUser randomAuthor = f.PickRandom(possibleAuthors);
                return randomAuthor.Id;
            });

        return fakeAuthorInStory.GenerateForever().Take(quantity).ToList();
    }
}
