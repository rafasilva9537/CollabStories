using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using api.Dtos.Story;
using api.Models;

namespace api.Mappers
{
    public static class StoryMappers
    {
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

        public static Expression<Func<Story, StoryMainInfoDto>> ToStoryMainInfoDto = (storyModel) => new StoryMainInfoDto
        {
            Id = storyModel.Id,
            Title = storyModel.Title,
            Description = storyModel.Description,
            CreatedDate = storyModel.CreatedDate,
            UpdatedDate = storyModel.UpdatedDate,
            MaximumAuthors = storyModel.MaximumAuthors,
        };


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
}