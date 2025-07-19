using System.Linq.Expressions;
using api.Dtos.AuthorInStory;
using api.Models;

namespace api.Mappers;

public class AuthorInStoryMappers
{
    public static readonly Expression<Func<AuthorInStory, AuthorFromStoryInListDto>> ProjectToAuthorFromStoryInListDto =
        (authorInStory) => 
            new AuthorFromStoryInListDto
            {
                AuthorUserName = authorInStory.Author.UserName,
                EntryDate = authorInStory.EntryDate,
            };
}