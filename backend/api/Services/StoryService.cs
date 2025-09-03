using api.Data;
using api.Dtos.AuthorInStory;
using api.Dtos.Pagination;
using api.Dtos.Story;
using api.Dtos.StoryPart;
using api.Exceptions;
using api.Interfaces;
using api.Mappers;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace api.Services;

public class StoryService : IStoryService
{
    private readonly ApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public StoryService(ApplicationDbContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<PagedKeysetStoryList<StoryMainInfoDto>> GetStoriesAsync(int? lastId = null, int pageSize = 15)
    {
        var storiesDto = await _context.Story
            .OrderByDescending(au => au.Id)
            .Where(u => !lastId.HasValue || u.Id <= lastId)
            .Take(pageSize + 1)
            .Select(StoryMappers.ProjectToStoryMainInfoDto)
            .AsNoTracking()
            .ToListAsync();
        
        bool hasMore = storiesDto.Count > pageSize;
        int? nextId = null;
        if (hasMore)
        {
            nextId = storiesDto[^1].Id;
            storiesDto.RemoveAt(storiesDto.Count - 1);
        }

        PagedKeysetStoryList<StoryMainInfoDto> pagedStories = new()
        {
            Items = storiesDto,
            HasMore = hasMore,
            NextId = nextId,
        };
        
        return pagedStories;
    }

    public async Task<StoryDto?> GetStoryAsync(int id)
    {
        StoryDto? storyDto = await _context.Story.Where(s => s.Id == id)
            .Select(StoryMappers.ProjectToStoryDto)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        return storyDto;
    }

    public async Task<StoryDto> CreateStoryAsync(CreateStoryDto createStoryDto, string username)
    {
        IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();
        
        Story storyModel = createStoryDto.ToCreateStoryModel();

        AppUser creatorUser = await _context.AppUser.FirstAsync(au => au.UserName == username);
        storyModel.UserId = creatorUser.Id;
        storyModel.CurrentAuthorId = creatorUser.Id;
        
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
        
        await transaction.CommitAsync();

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

    public async Task<string> ChangeToNextCurrentAuthorAsync(int storyId)
    {
        var storyAuthorsIds = await _context.Story
            .Where(s => s.Id == storyId)
            .Select(s => new
            {
                CurrentAuthorId = s.CurrentAuthorId,
                AuthorsInStoryIds = s.AuthorInStory
                    .OrderBy(ais => ais.EntryDate)
                    .Select(ais => ais.AuthorId)
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (storyAuthorsIds is null)
        {
            throw new StoryNotFoundException("Cannot change author: story does not exists.");
        }
        
        int? currentAuthorId = storyAuthorsIds.CurrentAuthorId;
        List<int> authorsIds = storyAuthorsIds.AuthorsInStoryIds;
        
        if(currentAuthorId is null) 
        {
            throw new InvalidOperationException("Cannot change author: no current author is set.");
        }
        if(storyAuthorsIds.AuthorsInStoryIds.Count <= 1) 
        {
            throw new InvalidOperationException("Cannot change author: story must have at least 2 authors.");
        }
        
        int currentAuthorIndex = authorsIds.IndexOf(currentAuthorId.Value);
        if (currentAuthorIndex == -1)
        {
            throw new InvalidOperationException("Cannot change author: current author is not in story.");
        }
        
        int nextAuthorIndex = (currentAuthorIndex + 1) % authorsIds.Count;
        int nextAuthorId = authorsIds[nextAuthorIndex];
        
        await _context.Story
            .Where(s => s.Id == storyId)
            .ExecuteUpdateAsync(sp => sp
                .SetProperty(story => story.CurrentAuthorId, nextAuthorId)
                .SetProperty(story => story.AuthorsMembershipChangeDate, _dateTimeProvider.UtcNow)
            );
        
        string newAuthorUsername = await _context.AppUser
            .Where(au => au.Id == nextAuthorId)
            .Select(au => au.UserName!)
            .FirstAsync();

        return newAuthorUsername;
    }

    public async Task<CompleteStoryDto?> GetCompleteStoryAsync(int storyId)
    {
        //TODO: improve, reduce roundtrips
        Story? completeStory =  await _context.Story
            .Where(story => story.Id == storyId)
            .Include(story => story.StoryParts)
            .FirstOrDefaultAsync();
        
        if(completeStory is null) return null;

        string currentAuthor = await _context.AppUser
            .Where(au => au.Id == completeStory.CurrentAuthorId)
            .Select(au => au.UserName)
            .FirstAsync();
        
        CompleteStoryDto completeStoryDto = completeStory.ToCompleteStoryDto();
        completeStoryDto.CurrentAuthor = currentAuthor;

        IList<AuthorFromStoryInListDto> storyAuthors = await _context.AuthorInStory
            .Where(ais => ais.StoryId == storyId)
            .Include(ais => ais.Author)
            .OrderBy(ais => ais.EntryDate)
            .Select(AuthorInStoryMappers.ProjectToAuthorFromStoryInListDto)
            .ToListAsync();
        
        completeStoryDto.StoryAuthors = storyAuthors;

        return completeStoryDto;
    }

    public async Task<string> GetCurrentAuthorUserNameAsync(int storyId)
    {
        string? currentAuthorUserName = await _context.Story
            .Where(s => s.Id == storyId)
            .Select(s => s.CurrentAuthor.UserName)
            .AsNoTracking()
            .FirstOrDefaultAsync();
        
        if(currentAuthorUserName is null) throw new StoryNotFoundException("Story does not exists.");
        
        return currentAuthorUserName;
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
            EntryDate = _dateTimeProvider.UtcNow
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

    public async Task<StoryInfoForSessionDto?> GetStoryInfoForSessionAsync(int storyId)
    {
        StoryInfoForSessionDto? storyInfo = await _context.Story
            .Where(s => s.Id == storyId)
            .Select(StoryMappers.ProjectToStoryInfoForSessionDto)
            .AsNoTracking()
            .FirstOrDefaultAsync();
        
        return storyInfo;
    }

    public async Task ChangeCurrentStoryAuthorAsync(int storyId, string username)
    {
        Story story = await _context.Story.FirstAsync(s => s.Id == storyId);
        
        int authorId = await _context.AppUser
            .Where(au => au.UserName == username)
            .Select(au => au.Id)
            .FirstAsync();

        story.CurrentAuthorId = authorId;
        await _context.SaveChangesAsync();
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