using System.Linq.Expressions;
using api.Dtos.Story;
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
        };
    }

    public static Expression<Func<Story, StoryDto>> ProjectToStoryDto = (storyModel) => new StoryDto
    {
        Id = storyModel.Id,
        Title = storyModel.Title,
        Description = storyModel.Title,
        CreatedDate = storyModel.CreatedDate,
        UpdatedDate = storyModel.UpdatedDate,
        MaximumAuthors = storyModel.MaximumAuthors,
    };

    public static StoryMainInfoDto ToStoryMainInfoDto(this Story storyModel)
    {
        return new StoryMainInfoDto
        {
            Id = storyModel.Id,
            Title = storyModel.Title,
            Description = storyModel.Title,
            CreatedDate = storyModel.CreatedDate,
            UpdatedDate = storyModel.UpdatedDate,
            MaximumAuthors = storyModel.MaximumAuthors,
        };
    }

    public static Expression<Func<Story, StoryMainInfoDto>> ProjectToStoryMainInfoDto = (storyModel) => new StoryMainInfoDto
    {
        Id = storyModel.Id,
        Title = storyModel.Title,
        Description = storyModel.Description,
        CreatedDate = storyModel.CreatedDate,
        UpdatedDate = storyModel.UpdatedDate,
        MaximumAuthors = storyModel.MaximumAuthors,
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
            StoryParts = storyModel.StoryParts.Select(storyPart => storyPart.ToStoryPartDto()).ToList(),
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