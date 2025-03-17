using api.Data;
using api.Dtos.Story;
using api.Dtos.StoryPart;
using api.Mappers;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Services;

    public interface IStoryService
    {
        Task<IList<StoryMainInfoDto>> GetStoriesAsync();
        Task<StoryDto?> GetStoryAsync(int id);
        Task<StoryDto> CreateStoryAsync(CreateStoryDto createStoryDto, string username);
        Task<bool> DeleteStoryAsync(int id);
        // TODO: possible addition of logged user parameter when service to controller validation is implemented
        Task<StoryDto?> UpdateStoryAsync(int storyId, UpdateStoryDto updateStoryDto);
        Task<bool> IsStoryCreator(string username, int storyId);
        Task<CompleteStoryDto?> GetCompleteStoryAsync(int StoryId);

        Task<StoryPartDto> CreateStoryPartAsync(int StoryId, string username, CreateStoryPartDto storyPartDto);
        Task<bool> DeleteStoryPart(int storyId, int storyPartId);
        Task<bool> IsStoryPartCreator(string username, int storyPartId);
    }

    public class StoryService : IStoryService
    {
        private readonly ApplicationDbContext _context;

        public StoryService(ApplicationDbContext context)
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

        public async Task<StoryDto> CreateStoryAsync(CreateStoryDto createStoryDto, string username)
        {
            Story storyModel = createStoryDto.ToCreateStoryModel();

            AppUser creatorUser = await _context.AppUser.FirstAsync(au => au.UserName == username);
            storyModel.UserId = creatorUser.Id;
            
            await _context.AddAsync(storyModel);
            await _context.SaveChangesAsync();

            StoryDto storyDto = storyModel.ToStoryDto();
            return storyDto;
        }

        public async Task<StoryDto?> UpdateStoryAsync(int storyId, UpdateStoryDto updateStoryDto)
        {
            Story? storyModel = await _context.Story.FirstOrDefaultAsync(s => s.Id == storyId);
            
            if(storyModel == null) return null;

            storyModel.UpdatedDate = DateTimeOffset.UtcNow;
            _context.Entry(storyModel).CurrentValues.SetValues(updateStoryDto);
            await _context.SaveChangesAsync();

            StoryDto storyDto = storyModel.ToStoryDto();
            storyDto.UserName = _context.AppUser
                                    .Where(au => au.Id == storyModel.UserId)
                                    .Select(au => au.UserName)
                                    .FirstOrDefault();

            return storyDto;
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

        // TODO: temporary solution, this method add unnecessary round trips to the database
        // this is will be used until service to controller validation is implemeneted
        public async Task<bool> IsStoryCreator(string username, int storyId)
        {
            int userId =  await _context.AppUser
                                    .Where(au => au.UserName == username)
                                    .Select(au => au.Id)
                                    .FirstAsync();

            bool IsStoryCreator = await _context.Story
                                            .Where(s => s.Id == storyId)
                                            .AnyAsync(s => s.UserId == userId);
            return IsStoryCreator;
        }


        public async Task<StoryPartDto> CreateStoryPartAsync(int StoryId, string username, CreateStoryPartDto storyPartDto)
        {
            StoryPart newStoryPart = storyPartDto.ToStoryPartModel();
            newStoryPart.StoryId = StoryId;

            AppUser creatorUser = await _context.AppUser
                                    .Where(au => au.UserName == username)
                                    .FirstAsync();
            newStoryPart.UserId = creatorUser.Id;

            await _context.AddAsync(newStoryPart);
            await _context.SaveChangesAsync();
            return newStoryPart.ToStoryPartDto();
        }

        public async Task<CompleteStoryDto?> GetCompleteStoryAsync(int StoryId)
        {
            Story? completeStory =  await _context.Story
                                            .Where(story => story.Id == StoryId)
                                            .Include(story => story.StoryParts)
                                            .FirstOrDefaultAsync();
            
            if(completeStory == null) return null;

            CompleteStoryDto completeStoryDto = completeStory.ToCompleteStoryDto();

            IList<StoryPartInListDto> storyPartsDto = await _context.StoryPart
                                            .Where(sp => sp.StoryId == StoryId)
                                            .Include(sp => sp.User)
                                            .Select(StoryPartMappers.ProjectToStoryPartInListDto)
                                            .ToListAsync();
            completeStoryDto.StoryParts = storyPartsDto;

            return completeStoryDto;
        }

        public async Task<bool> DeleteStoryPart(int storyId, int storyPartId)
        {
            StoryPart? deletedStoryPart = _context.StoryPart.FirstOrDefault(storyPart => storyPart.Id == storyPartId);

            if(deletedStoryPart == null || deletedStoryPart.StoryId != storyId) return false;

            _context.Remove(deletedStoryPart);
            await _context.SaveChangesAsync();
            return true;
        }

        // TODO: temporary solution, this method add unnecessary round trips to the database
        // this is will be used until service to controller validation is implemeneted
        public async Task<bool> IsStoryPartCreator(string username, int storyPartId)
        {
            int userId =  await _context.AppUser
                                    .Where(au => au.UserName == username)
                                    .Select(au => au.Id)
                                    .FirstAsync();

            bool isStoryPartCreator = await _context.StoryPart
                                            .Where(s => s.Id == storyPartId)
                                            .AnyAsync(s => s.UserId == userId);
            return isStoryPartCreator;
        }
    }