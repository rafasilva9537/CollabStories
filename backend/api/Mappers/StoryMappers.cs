using System.Linq.Expressions;
using api.Dtos.Story;
using api.Dtos.StoryPart;
using api.Models;

namespace api.Mappers;

// EF Core need to receive expressions, it's not able to translate methods to SQL
// So, every mapper inside a select needs to have its expression version
public static class StoryMappers
{
    // Model to Dto
    
    /// <summary>
    /// Maps a <see cref="Story"/> model to a <see cref="StoryDto"/> data transfer object.
    /// </summary>
    /// <param name="storyModel">The <see cref="Story"/> model to be mapped.</param>
    /// <param name="userName">The username to include in the <see cref="StoryDto"/>.
    /// If null, the username associated with the <see cref="Story.User"/> will be used.
    /// If <see cref="Story.User"/> is null, the username will be null.</param>
    /// <returns>A <see cref="StoryDto"/> object containing the mapped data from the <see cref="Story"/> model.</returns>
    public static StoryDto ToStoryDto(this Story storyModel, string? userName = null)
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
            UserName = userName ?? storyModel.User?.UserName
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
        UserName = storyModel.User!.UserName,
        TurnDurationSeconds = storyModel.TurnDurationSeconds,
    };

    /// <summary>
    /// Maps a <see cref="Story"/> model to a <see cref="StoryMainInfoDto"/> data transfer object.
    /// </summary>
    /// <param name="storyModel">The <see cref="Story"/> model to be mapped.</param>
    /// <param name="userName">The username to include in the <see cref="StoryMainInfoDto"/>.
    /// If null, the username associated with the <see cref="Story.User"/> will be used.
    /// If <see cref="Story.User"/> is null, the username will be null.</param>
    /// <returns>A <see cref="StoryMainInfoDto"/> object containing the mapped data from the <see cref="Story"/> model.</returns>
    public static StoryMainInfoDto ToStoryMainInfoDto(this Story storyModel, string? userName = null)
    {
        return new StoryMainInfoDto
        {
            Id = storyModel.Id,
            Title = storyModel.Title,
            Description = storyModel.Description,
            CreatedDate = storyModel.CreatedDate,
            UpdatedDate = storyModel.UpdatedDate,
            MaximumAuthors = storyModel.MaximumAuthors,
            UserName = userName ?? storyModel.User?.UserName,
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
            UserName = storyModel.User!.UserName,
        };

    public static readonly Expression<Func<Story, StoryInfoForSessionDto>> ProjectToStoryInfoForSessionDto =
        (storyModel) => new StoryInfoForSessionDto()
        {
            TurnDurationSeconds = storyModel.TurnDurationSeconds,
            AuthorsMembershipChangeDate = storyModel.UpdatedDate,
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
            CurrentAuthor = storyModel.CurrentAuthor?.UserName,
            StoryOwner = storyModel.User?.UserName
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