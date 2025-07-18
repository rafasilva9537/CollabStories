using System.Collections;
using api.Models;

namespace api.IntegrationTests.Data;

public class AuthorsInStoryTestData : IEnumerable<object[]>
{
    public const int StoryId = 158;
    public static readonly DateTimeOffset AuthorsMembershipChangeDate = DateTimeOffset.Parse("2024-12-05T05:09:10.8676393+00:00");
    
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[]
        {
            new TestUserModel
            {
                UserName = "neva_rosenbaum29",
                NameIdentifier = "neva_rosenbaum29",
                Email = "neva_rosenbaum2930@hotmail.com"
            },
            new TestUserModel
            {
                UserName = "jovani.lueilwitz",
                NameIdentifier = "jovani.lueilwitz",
                Email = "jovani.lueilwitz77@gmail.com"
            },
            new TestUserModel()
            {
                UserName = "fabian.fisher",
                NameIdentifier = "fabian.fisher",
                Email = "fabian.fisher.herman6@yahoo.com"
            },
            new Story()
            {
                Id = 158,
                UserId = 6,
                Title = "Deserunt dolorem.",
                Description = "Quo provident nihil quod aliquid provident voluptas atque delectus dolorem eius.",
                MaximumAuthors = 9,
                TurnDurationSeconds = 1447,
                CreatedDate = DateTimeOffset.Parse("2024-09-10T10:12:08.6461789+00:00"),
                UpdatedDate = DateTimeOffset.Parse("2024-12-05T05:09:10.8676393+00:00"),
                AuthorsMembershipChangeDate = AuthorsMembershipChangeDate,
                IsFinished = false
            }
        };
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}