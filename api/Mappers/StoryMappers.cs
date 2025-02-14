using System;
using System.Collections.Generic;
using System.Linq;
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