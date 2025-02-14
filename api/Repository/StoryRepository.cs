using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Dtos.Story;
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
            return await _context.Story.Select(s => s.ToStoryMainInfoDto()).ToListAsync();
        }

        public async Task<StoryDto?> GetStoryAsync(int id)
        {
            return await _context.Story.Where(s => s.Id == id).Select(s => s.ToStoryDto()).FirstOrDefaultAsync();
        }

        public async Task<StoryDto> CreateStoryAsync(CreateStoryDto createStoryDto)
        {
            Story storyModel = createStoryDto.ToCreateStoryModel();
            await _context.AddAsync(storyModel);
            await _context.SaveChangesAsync();
            return storyModel.ToStoryDto();
        }
    }
}