using api.Constants;
using api.Models;
using Bogus;
using Microsoft.IdentityModel.Tokens;

namespace api.Data.Seed;

public class FakeDataGenerator
{
    private readonly int _seed;
    private readonly DateTimeOffset _userStartDate;
    private readonly DateTimeOffset _userEndDate;
    private readonly DateTimeOffset _storyStartDate;
    private readonly DateTimeOffset _storyEndDate;

    public FakeDataGenerator(int? seed = null)
    {
        _seed = seed ?? new Random().Next();
        _userStartDate = new(2022, 1, 1, 0, 0, 0, new TimeSpan(0));
        _userEndDate = _userStartDate.AddYears(3);

        _storyStartDate = _userStartDate.AddHours(1);
        _storyEndDate = _userEndDate.AddDays(40);
    }

    public List<AppUser> GenerateAppUsers(int quantity)
    {
        Faker<AppUser> fakeAppUser = new Faker<AppUser>()
            .UseSeed(_seed)
            .RuleFor(au => au.NickName, f => f.Name.FirstName())
            .RuleFor(au => au.UserName, (f, au) => f.Internet.UserName().ToLower())
            .RuleFor(au => au.NormalizedUserName, (f, au) => au.UserName.ToUpper())
            .RuleFor(au => au.Description, f => f.Lorem.Sentence(5, 15))
            .RuleFor(au => au.Email, (f, au) => f.Internet.Email(au.UserName).ToLower())
            .RuleFor(au => au.NormalizedEmail, (f, au) => au.Email.ToUpper())
            .RuleFor(au => au.CreatedDate, f => f.Date.BetweenOffset(_userStartDate, _userEndDate))
            .RuleFor(
                au => au.ProfileImage, f => Path.Combine(
                    DirectoryPathConstants.Media,
                    DirectoryPathConstants.Images,
                    DirectoryPathConstants.ProfileImages,
                    f.Random.Guid().ToString()
                )
            );

        List<AppUser> appUsers = fakeAppUser.Generate(quantity).ToList();
        return appUsers;
    }

    public List<Story> GenerateStories(int quantity, List<AppUser>? possibleAuthors = null)
    {
        Faker<Story> fakeStory = new Faker<Story>()
            .UseSeed(_seed)
            .RuleFor(s => s.Title, f => f.Lorem.Sentence(2, 6))
            .RuleFor(s => s.Description, f => f.Lorem.Sentence(8, 12))
            .RuleFor(s => s.MaximumAuthors, f => f.Random.Int(2, StoryConstants.MaxAuthors))
            .RuleFor(s => s.TurnDurationSeconds, f => f.Random.Int(30, 3600))
            .RuleFor(s => s.CreatedDate, f => f.Date.BetweenOffset(_storyStartDate, _storyEndDate))
            .RuleFor(s => s.UpdatedDate, (f, s) => f.Date.BetweenOffset(s.CreatedDate, _storyEndDate))
            .RuleFor(s => s.AuthorsMembershipChangeDate, (f, s) => s.UpdatedDate)
            .RuleFor(s => s.UserId, f =>
            {
                if (possibleAuthors.IsNullOrEmpty()) return null;
                AppUser randomUser = f.PickRandom(possibleAuthors);
                return randomUser.Id;
            })
            .RuleFor(s => s.CurrentAuthorId, (f, s) => s.UserId);

        if (!possibleAuthors.IsNullOrEmpty())
        {
            fakeStory = fakeStory.RuleFor(s => s.AuthorsInStory, (f, s) =>
            {
                int authorsQuantity = f.Random.Int(1, 7);
                
                List<AuthorInStory> newAuthors = GenerateAuthorsInStory(authorsQuantity, s.Id, possibleAuthors!);
                
                // story owner
                if (s.UserId is not null && s.UserId != 0)
                {
                    int ownerId = (int)s.UserId;
                    newAuthors.Add(new AuthorInStory
                    {
                        AuthorId = ownerId,
                        StoryId = s.Id,
                        EntryDate = s.CreatedDate
                    });
                }

                List<AuthorInStory> authorInStories = newAuthors
                    .DistinctBy(a => (a.AuthorId, a.StoryId))
                    .ToList();
                s.AuthorsInStory.AddRange(authorInStories);
                return s.AuthorsInStory;
            });
        }

        List<Story> stories = fakeStory.Generate(quantity).ToList();
        return stories;
    }
    
    public List<StoryPart> GenerateStoryParts(int quantity, List<Story> possibleStories)
    {
        Faker<StoryPart> fakeStoryPart = new Faker<StoryPart>()
            .UseSeed(_seed)
            .RuleFor(sp => sp.Text, f => f.Lorem.Sentence(3, 100))
            .RuleFor(sp => sp.CreatedDate, f => f.Date.BetweenOffset(_storyStartDate, _storyEndDate))
            .RuleFor(sp => sp.StoryId, f =>
            {
                Story randomStory = f.PickRandom(possibleStories);
                return randomStory.Id;
            })
            .RuleFor(sp => sp.UserId, (f, sp) =>
            {
                List<int> authorsInStory = possibleStories
                    .Where(story => story.Id == sp.StoryId)
                    .SelectMany(story => story.AuthorsInStory)
                    .Select(ais => ais.AuthorId)
                    .ToList();
                
                if (authorsInStory.IsNullOrEmpty()) return null;
                int randomAuthorId = f.PickRandom(authorsInStory);
                return randomAuthorId;
            });

        List<StoryPart> storyParts = fakeStoryPart.Generate(quantity).ToList();
        return storyParts;
    }

    private List<AuthorInStory> GenerateAuthorsInStory(int quantity, int storyId, List<AppUser> possibleAuthors)
    {
        int authorSeed = HashCode.Combine(_seed, storyId);
        
        Faker<AuthorInStory> fakeAuthorInStory = new Faker<AuthorInStory>()
            .UseSeed(authorSeed)
            .RuleFor(ais => ais.EntryDate, (f, ais) => f.Date.BetweenOffset(_storyStartDate, _storyEndDate))
            .RuleFor(ais => ais.StoryId, f => storyId)
            .RuleFor(ais => ais.AuthorId, f =>
            {
                AppUser randomAuthor = f.PickRandom(possibleAuthors);
                return randomAuthor.Id;
            });

        return fakeAuthorInStory.Generate(quantity).ToList();
    }
}
