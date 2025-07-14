using System.Linq.Expressions;
using api.Dtos.Story;
using api.Dtos.StoryPart;
using api.Models;

namespace api.Mappers;

// EF Core need to receive expressions, it's not able to translate methods to SQL
// So, every mapper inside a select need to have it's expression version
public static class StoryMappers
{
    // Model to Dto
    public static StoryDto ToStoryDto(this Story storyModel)
    {
        return new StoryDto
        {
            Id = storyModel.Id,
            Title = storyModel.Title,
            Description = storyModel.Description,
            CreatedDate = storyModel.CreatedDate,
            UpdatedDate = storyModel.UpdatedDate,
            MaximumAuthors = storyModel.MaximumAuthors,
            TurnDurationSeconds = storyModel.TurnDurationSeconds,
            UserName = storyModel.User?.UserName
        };
    }

    public static readonly Expression<Func<Story, StoryDto>> ProjectToStoryDto = (storyModel) => new StoryDto
    {
        Id = storyModel.Id,
        Title = storyModel.Title,
        Description = storyModel.Description,
        CreatedDate = storyModel.CreatedDate,
        UpdatedDate = storyModel.UpdatedDate,
        MaximumAuthors = storyModel.MaximumAuthors,
        UserName = storyModel.User.UserName,
        TurnDurationSeconds = storyModel.TurnDurationSeconds,
    };

    public static StoryMainInfoDto ToStoryMainInfoDto(this Story storyModel)
    {
        return new StoryMainInfoDto
        {
            Id = storyModel.Id,
            Title = storyModel.Title,
            Description = storyModel.Description,
            CreatedDate = storyModel.CreatedDate,
            UpdatedDate = storyModel.UpdatedDate,
            MaximumAuthors = storyModel.MaximumAuthors,
            UserName = storyModel?.User?.UserName,
        };
    }

    public static readonly Expression<Func<Story, StoryMainInfoDto>> ProjectToStoryMainInfoDto = (storyModel) =>
        new StoryMainInfoDto
        {
            Id = storyModel.Id,
            Title = storyModel.Title,
            Description = storyModel.Description,
            CreatedDate = storyModel.CreatedDate,
            UpdatedDate = storyModel.UpdatedDate,
            MaximumAuthors = storyModel.MaximumAuthors,
            UserName = storyModel.User.UserName,
        };

    public static readonly Expression<Func<Story, StoryInfoForSessionDto>> ProjectToStoryInfoForSessionDto =
        (storyModel) => new StoryInfoForSessionDto()
        {
            TurnDurationSeconds = storyModel.TurnDurationSeconds,
            UpdatedDate = storyModel.UpdatedDate,
        };

    public static CompleteStoryDto ToCompleteStoryDto(this Story storyModel)
    {
        return new CompleteStoryDto
        {
            Id = storyModel.Id,
            Title = storyModel.Title,
            Description = storyModel.Description,
            CreatedDate = storyModel.CreatedDate,
            UpdatedDate = storyModel.UpdatedDate,
            MaximumAuthors = storyModel.MaximumAuthors,
            TurnDurationSeconds = storyModel.TurnDurationSeconds,
            StoryParts = new List<StoryPartInListDto>(),
        };
    }


    // Dto to Model
    public static Story ToCreateStoryModel(this CreateStoryDto storyDto)
    {
        return new Story
        {
            Title = storyDto.Title,
            Description = storyDto.Title,
            MaximumAuthors = storyDto.MaximumAuthors,
            TurnDurationSeconds = storyDto.TurnDurationSeconds,
        };
    }
}