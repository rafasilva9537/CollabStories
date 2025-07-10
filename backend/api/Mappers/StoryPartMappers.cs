using System.Linq.Expressions;
using api.Dtos.StoryPart;
using api.Models;

namespace api.Mappers;

public static class StoryPartMappers
{
    // Dto to Model
    public static StoryPart ToStoryPartModel(this StoryPartDto storyPartDto)
    {
        return new StoryPart
        {
            Id = storyPartDto.Id,
            Text = storyPartDto.Text,
            CreatedDate = storyPartDto.CreatedDate,
            StoryId = storyPartDto.StoryId,
        };
    }

    public static StoryPart ToStoryPartModel(this CreateStoryPartDto storyPartDto)
    {
        return new StoryPart
        {
            Text = storyPartDto.Text,
        };
    }


    // Model to Dto
    public static StoryPartDto ToStoryPartDto(this StoryPart storyPart)
    {
        return new StoryPartDto
        {
            Id = storyPart.Id,
            Text = storyPart.Text,
            CreatedDate = storyPart.CreatedDate,
            StoryId = storyPart.StoryId,
            UserName = storyPart.User.UserName,
        };
    }
    
    public static readonly Expression<Func<StoryPart, StoryPartInListDto>> ProjectToStoryPartInListDto = (storyPartModel) => new StoryPartInListDto 
    {
        Id = storyPartModel.Id,
        UserName = storyPartModel.User.UserName,
        Text = storyPartModel.Text,
        CreatedDate = storyPartModel.CreatedDate,
    };
}