using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Dtos.Story;
using api.Dtos.StoryPart;
using api.Mappers;
using api.Models;
using Microsoft.EntityFrameworkCore;


namespace api.Repository
{
    public interface IStoryRepository
    {
        Task<IList<StoryMainInfoDto>> GetStoriesAsync();
        Task<StoryDto?> GetStoryAsync(int id);
        Task<StoryDto> CreateStoryAsync(CreateStoryDto createStoryDto);
        Task<bool> DeleteStoryAsync(int id);
        Task<StoryDto?> UpdateStoryAsync(int id, UpdateStoryDto updateStoryDto);
        Task<CompleteStoryDto?> GetCompleteStoryAsync(int StoryId);

        Task<StoryPartDto> CreateStoryPartAsync(int StoryId, CreateStoryPartDto storyPartDto);
        Task<bool> DeleteStoryPart(int storyId, int storyPartId);
    }

    public class StoryRepository : IStoryRepository
    {
        private readonly ApplicationDBContext _context;

        public StoryRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<IList<StoryMainInfoDto>> GetStoriesAsync()
        {   
            return await _context.Story.Select(StoryMappers.ProjectToStoryMainInfoDto).ToListAsync();
        }

        public async Task<StoryDto?> GetStoryAsync(int id)
        {
            return await _context.Story.Where(s => s.Id == id).Select(StoryMappers.ProjectToStoryDto).FirstOrDefaultAsync();
        }

        public async Task<StoryDto> CreateStoryAsync(CreateStoryDto createStoryDto)
        {
            Story storyModel = createStoryDto.ToCreateStoryModel();
            await _context.AddAsync(storyModel);
            await _context.SaveChangesAsync();
            return storyModel.ToStoryDto();
        }

        public async Task<StoryDto?> UpdateStoryAsync(int id, UpdateStoryDto updateStoryDto)
        {
            Story? storyModel = await _context.Story.FirstOrDefaultAsync(story => story.Id == id);
            
            if(storyModel == null) return null;

            storyModel.UpdatedDate = DateTimeOffset.UtcNow;
            _context.Entry(storyModel).CurrentValues.SetValues(updateStoryDto);
            await _context.SaveChangesAsync();

            return storyModel.ToStoryDto();
        }

        public async Task<bool> DeleteStoryAsync(int id)
        {
            Story? deletedStory = await _context.Story.Where(story => story.Id == id).FirstOrDefaultAsync();

            if(deletedStory == null)
            {
                return false;
            }

            _context.Story.Remove(deletedStory);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<StoryPartDto> CreateStoryPartAsync(int StoryId, CreateStoryPartDto storyPartDto)
        {
            StoryPart newStoryPart = storyPartDto.ToStoryPartModel();
            newStoryPart.StoryId = StoryId;
            await _context.AddAsync(newStoryPart);
            await _context.SaveChangesAsync();
            return newStoryPart.ToStoryPartDto();
        }

        public async Task<CompleteStoryDto?> GetCompleteStoryAsync(int StoryId)
        {
            Story? completeStory =  await _context.Story.Where(story => story.Id == StoryId).Include(story => story.StoryParts).FirstOrDefaultAsync();
            
            if(completeStory == null) return null;

            return completeStory.ToCompleteStoryDto();
        }

        public async Task<bool> DeleteStoryPart(int storyId, int storyPartId)
        {
            StoryPart? deletedStoryPart = _context.StoryPart.FirstOrDefault(storyPart => storyPart.Id == storyPartId);

            if(deletedStoryPart == null || deletedStoryPart.StoryId != storyId) return false;

            _context.Remove(deletedStoryPart);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}