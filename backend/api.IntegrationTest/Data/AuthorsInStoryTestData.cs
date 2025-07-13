using System.Collections;

namespace api.IntegrationTests.Data;

public class AuthorsInStoryTestData : IEnumerable<object[]>
{
    public const int StoryId = 158;
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
            }
        };
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}