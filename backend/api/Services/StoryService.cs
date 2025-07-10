using api.Data;
using api.Dtos.AuthorInStory;
using api.Dtos.Story;
using api.Dtos.StoryPart;
using api.Mappers;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Services;

public interface IStoryService
{
    Task<IList<StoryMainInfoDto>> GetStoriesAsync(int? lastId);
    Task<StoryDto?> GetStoryAsync(int id);
    Task<StoryDto> CreateStoryAsync(CreateStoryDto createStoryDto, string username);
    Task<bool> DeleteStoryAsync(int id);
    // TODO: possible addition of logged user parameter when service to controller validation is implemented
    Task<StoryDto?> UpdateStoryAsync(int storyId, UpdateStoryDto updateStoryDto);
    Task<bool> IsStoryCreator(string username, int storyId);
    Task<CompleteStoryDto?> GetCompleteStoryAsync(int storyId);
    Task<bool> JoinStoryAsync(string username, int storyId);
    Task<bool> LeaveStoryAsync(string username, int storyId);
    Task<bool> IsStoryAuthorAsync(string username, int storyId);
    Task<bool> StoryExistsAsync(int storyId);

    Task<StoryPartDto?> CreateStoryPartAsync(int storyId, string username, CreateStoryPartDto storyPartDto);
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

    public async Task<IList<StoryMainInfoDto>> GetStoriesAsync(int? lastId)
    {
        int pageSize = 15;

        List<StoryMainInfoDto> storyDto = await _context.Story
            .OrderByDescending(s => s.Id)
            .Where(s => !lastId.HasValue || s.Id < lastId)
            .Take(pageSize)
            .Select(StoryMappers.ProjectToStoryMainInfoDto)
            .ToListAsync();

        return storyDto;
    }

    public async Task<StoryDto?> GetStoryAsync(int id)
    {
        StoryDto? storyDto = await _context.Story.Where(s => s.Id == id)
            .Select(StoryMappers.ProjectToStoryDto)
            .FirstOrDefaultAsync();

        return storyDto;
    }

    public async Task<StoryDto> CreateStoryAsync(CreateStoryDto createStoryDto, string username)
    {
        // TODO: Add data atomicity. Wrap all in a single transaction
        Story storyModel = createStoryDto.ToCreateStoryModel();

        AppUser creatorUser = await _context.AppUser.FirstAsync(au => au.UserName == username);
        storyModel.UserId = creatorUser.Id;
        
        await _context.AddAsync(storyModel);
        await _context.SaveChangesAsync();

        AuthorInStory authorInStory = new AuthorInStory 
        {
            AuthorId = creatorUser.Id, 
            StoryId = storyModel.Id, 
            EntryDate = DateTimeOffset.UtcNow 
        };
        await _context.AuthorInStory.AddAsync(authorInStory);
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
    // this is will be used until service to controller validation is implemented
    public async Task<bool> IsStoryCreator(string username, int storyId)
    {
        int userId =  await _context.AppUser
            .Where(au => au.UserName == username)
            .Select(au => au.Id)
            .FirstAsync();

        bool isStoryCreator = await _context.Story
            .Where(s => s.Id == storyId)
            .AnyAsync(s => s.UserId == userId);
        return isStoryCreator;
    }

    public async Task<CompleteStoryDto?> GetCompleteStoryAsync(int storyId)
    {
        //TODO: improve, reduce roundtrips
        Story? completeStory =  await _context.Story
                                    .Where(story => story.Id == storyId)
                                    .Include(story => story.StoryParts)
                                    .FirstOrDefaultAsync();
        
        if(completeStory == null) return null;

        CompleteStoryDto completeStoryDto = completeStory.ToCompleteStoryDto();

        IList<StoryPartInListDto> storyPartsDto = await _context.StoryPart
            .Where(sp => sp.StoryId == storyId)
            .Include(sp => sp.User)
            .Select(StoryPartMappers.ProjectToStoryPartInListDto)
            .ToListAsync();

        IList<AuthorFromStoryInListDto> storyAuthors = await _context.AuthorInStory
            .Where(ais => ais.StoryId == storyId)
            .Include(ais => ais.Author)
            // TODO: create mapper for projection here
            .Select(ais => new AuthorFromStoryInListDto
            {
                AuthorUserName = ais.Author.UserName,
                EntryDate = ais.EntryDate,
            })
            .ToListAsync();

        completeStoryDto.StoryParts = storyPartsDto;
        completeStoryDto.StoryAuthors = storyAuthors;

        return completeStoryDto;
    }

    public async Task<bool> JoinStoryAsync(string username, int storyId)
    {
        // TODO: decrease roundtrips
        int userId = await _context.AppUser
                        .Where(au => au.UserName == username)
                        .Select(au => au.Id)
                        .FirstAsync();
        
        bool storyExists = await _context.Story
                            .Where(s => s.Id == storyId)
                            .AnyAsync();

        bool authorInStoryExists = await _context.AuthorInStory
                                        .AnyAsync(ais => ais.AuthorId == userId && ais.StoryId == storyId);
        if(!storyExists || authorInStoryExists) return false;

        AuthorInStory authorInStory = new AuthorInStory 
        {
            AuthorId = userId, 
            StoryId = storyId, 
            EntryDate = DateTimeOffset.UtcNow 
        };

        await _context.AuthorInStory.AddAsync(authorInStory);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> LeaveStoryAsync(string username, int storyId)
    {
        // TODO: decrease roundtrips
        int userId = await _context.AppUser
                        .Where(au => au.UserName == username)
                        .Select(au => au.Id)
                        .FirstAsync();

        bool storyExists = await _context.Story
                            .Where(s => s.Id == storyId)
                            .AnyAsync();
        if(!storyExists) return false;

        AuthorInStory? authorInStory = await _context.AuthorInStory
                                        .FirstOrDefaultAsync(ais => ais.AuthorId == userId && ais.StoryId == storyId);
        if(authorInStory is null) return false;

        _context.Remove(authorInStory);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> IsStoryAuthorAsync(string username, int storyId)
    {
        int? userId = await _context.AppUser
                        .Where(au => au.UserName == username)
                        .Select(au => au.Id)
                        .FirstOrDefaultAsync();

        if(userId is null) return false;

        bool isInStory = await _context.AuthorInStory
                            .AnyAsync(ais => ais.StoryId == storyId && ais.AuthorId == userId);
        return isInStory;
    }

    public async Task<bool> StoryExistsAsync(int storyId)
    {
        return await _context.Story.AnyAsync(s => s.Id == storyId);
    }

    public async Task<StoryPartDto?> CreateStoryPartAsync(int storyId, string username, CreateStoryPartDto storyPartDto)
    {
        StoryPart newStoryPart = storyPartDto.ToStoryPartModel();
        newStoryPart.StoryId = storyId;

        AppUser creatorUser = await _context.AppUser
                                .Where(au => au.UserName == username)
                                .FirstAsync();
                                
        AuthorInStory? authorInStory = await _context.AuthorInStory
                            .FirstOrDefaultAsync(ais => ais.AuthorId == creatorUser.Id && ais.StoryId == storyId);

        if(authorInStory is null) return null;

        newStoryPart.UserId = creatorUser.Id;
        await _context.AddAsync(newStoryPart);
        await _context.SaveChangesAsync();
        return newStoryPart.ToStoryPartDto();
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