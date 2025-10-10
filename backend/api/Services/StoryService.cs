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
        List<StoryMainInfoDto> storiesDto = await _context.Story
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
        StoryDto? storyDto = await _context.Story
            .Where(s => s.Id == id)
            .Select(StoryMappers.ProjectToStoryDto)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        return storyDto;
    }

    public async Task<StoryDto> CreateStoryAsync(CreateStoryDto createStoryDto, string userName)
    {
        await using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();
        
        Story storyModel = createStoryDto.ToCreateStoryModel();
        
        int creatorId = await _context.AppUser
            .Where(au => au.UserName == userName)
            .Select(au => au.Id)
            .FirstOrDefaultAsync();
        if(creatorId == 0) throw new UserNotFoundException($"User does not exist with username: {userName}.");
        
        storyModel.UserId = creatorId;
        storyModel.CurrentAuthorId = creatorId;
        
        DateTimeOffset dateNow = _dateTimeProvider.UtcNow;
        storyModel.CreatedDate = dateNow;
        storyModel.UpdatedDate = dateNow;
        storyModel.AuthorsMembershipChangeDate = dateNow;
        
        AuthorInStory authorInStory = new() 
        {
            AuthorId = creatorId, 
            StoryId = storyModel.Id,
            EntryDate = _dateTimeProvider.UtcNow
        };
        storyModel.AuthorsInStory.Add(authorInStory);
        
        await _context.AddAsync(storyModel);
        await _context.SaveChangesAsync();
        
        await transaction.CommitAsync();
        
        StoryDto storyDto = storyModel.ToStoryDto(userName);
        return storyDto;
    }

    public async Task<StoryDto> UpdateStoryAsync(int storyId, UpdateStoryDto updateStoryDto)
    {
        Story? storyModel = await _context.Story.FirstOrDefaultAsync(s => s.Id == storyId);
        
        if(storyModel is null) throw new StoryNotFoundException("Story does not exist.");

        storyModel.UpdatedDate = _dateTimeProvider.UtcNow;
        
        bool titleIsNullOrEmpty = string.IsNullOrEmpty(updateStoryDto.Title);
        bool descriptionIsNull = updateStoryDto.Description is null;
        bool maximumAuthorsIsNull = updateStoryDto.MaximumAuthors is null;
        bool turnDurationSecondsIsNull = updateStoryDto.TurnDurationSeconds is null;
        
        if(!titleIsNullOrEmpty) storyModel.Title = updateStoryDto.Title!;
        if(!descriptionIsNull) storyModel.Description = updateStoryDto.Description!;
        if(!maximumAuthorsIsNull) storyModel.MaximumAuthors = (int)updateStoryDto.MaximumAuthors!;
        if(!turnDurationSecondsIsNull)
        {
            storyModel.TurnDurationSeconds = (int)updateStoryDto.TurnDurationSeconds!;
            // When turn duration is changed, the membership date should change because the story session timer uses that as a reference.
            storyModel.AuthorsMembershipChangeDate = _dateTimeProvider.UtcNow;
        }
        
        await _context.SaveChangesAsync();

        string? userName = await _context.AppUser
            .Where(au => au.Id == storyModel.UserId)
            .Select(au => au.UserName)
            .FirstOrDefaultAsync();
        StoryDto storyDto = storyModel.ToStoryDto(userName);

        return storyDto;
    }
    
    public async Task<bool> DeleteStoryAsync(int id)
    {
        Story? storyToDelete = await _context.Story
            .Where(story => story.Id == id)
            .FirstOrDefaultAsync();
        if (storyToDelete is null) return false;

        _context.Story.Remove(storyToDelete);
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> IsStoryOwner(string userName, int storyId)
    {
        bool isStoryCreator = await _context.Story
            .AnyAsync(s => s.User != null && s.User.UserName == userName && s.Id == storyId);
        
        return isStoryCreator;
    }

    public async Task<string> ChangeToNextCurrentAuthorAsync(int storyId)
    {
        var storyAuthorsIds = await _context.Story
            .Where(s => s.Id == storyId)
            .Select(s => new
            {
                CurrentAuthorId = s.CurrentAuthorId,
                AuthorsInStoryIds = s.AuthorsInStory
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
            string currentAuthorUsername = await _context.AppUser
                .Where(au => au.Id == currentAuthorId)
                .Select(au => au.UserName!)
                .FirstAsync();
            return currentAuthorUsername;
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
        CompleteStoryDto? completeStoryDto =  await _context.Story
            .AsNoTracking()
            .Where(story => story.Id == storyId)
            .Select(s => new CompleteStoryDto()
            {
                Id = s.Id,
                Title = s.Title,
                Description = s.Description,
                CreatedDate = s.CreatedDate,
                UpdatedDate = s.UpdatedDate,
                MaximumAuthors = s.MaximumAuthors,
                TurnDurationSeconds = s.TurnDurationSeconds,
                CurrentAuthor = s.CurrentAuthor != null ? s.CurrentAuthor.UserName : null,
                StoryOwner = s.User != null ? s.User.UserName : null,
                StoryAuthors = s.AuthorsInStory
                    .OrderBy(ais => ais.EntryDate)
                    .Select(ais => new AuthorFromStoryInListDto()
                    {
                        AuthorUserName = ais.Author.UserName,
                        EntryDate = ais.EntryDate,
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        return completeStoryDto;
    }

    public async Task<string?> GetCurrentAuthorUserNameAsync(int storyId)
    {
        bool storyExists = await _context.Story.AnyAsync(s => s.Id == storyId);
        if(!storyExists) throw new StoryNotFoundException("Story does not exist.");
        
        string? currentAuthorUserName = await _context.Story
            .AsNoTracking()
            .Where(s => s.Id == storyId)
            .Select(s => s.CurrentAuthor != null ? s.CurrentAuthor.UserName : null)
            .FirstOrDefaultAsync();
        
        return currentAuthorUserName;
    }
    
    public async Task<bool> JoinStoryAsync(string userName, int storyId)
    {
        var userAndStory = await _context.AppUser
            .Where(au => au.UserName == userName)
            .Select(au => new
            {
                UserId = au.Id,
                StoryExists = _context.Story.Any(s => s.Id == storyId)
            })
            .FirstOrDefaultAsync();
        
        if(userAndStory is null) throw new UserNotFoundException($"User does not exist with username: {userName}.");
        if(!userAndStory.StoryExists) throw new StoryNotFoundException("Story does not exist.");

        int userId = userAndStory.UserId;
        bool authorInStoryExists = await _context.AuthorInStory
                        .AnyAsync(ais => ais.AuthorId == userId && ais.StoryId == storyId);
        if(authorInStoryExists) return false;

        AuthorInStory authorInStory = new() 
        {
            AuthorId = userId, 
            StoryId = storyId, 
            EntryDate = _dateTimeProvider.UtcNow
        };

        await _context.AuthorInStory.AddAsync(authorInStory);
        await _context.SaveChangesAsync();

        return true;
    }
    
    public async Task<bool> LeaveStoryAsync(string userName, int storyId)
    {
        var userAndStory = await _context.AppUser
            .Where(au => au.UserName == userName)
            .Select(au => new
            {
                UserId = au.Id,
                StoryExists = _context.Story.Any(s => s.Id == storyId)
            })
            .FirstOrDefaultAsync();
        
        if(userAndStory is null) throw new UserNotFoundException($"User does not exist with username: {userName}.");
        if(!userAndStory.StoryExists) throw new StoryNotFoundException("Story does not exist.");
        
        int userId = userAndStory.UserId;
        AuthorInStory? authorInStory = await _context.AuthorInStory 
            .FirstOrDefaultAsync(ais => ais.AuthorId == userId && ais.StoryId == storyId);
        if(authorInStory is null) return false;

        _context.Remove(authorInStory);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> IsStoryAuthorAsync(string userName, int storyId)
    {
        int userId = await _context.AppUser
            .Where(au => au.UserName == userName)
            .Select(au => au.Id)
            .FirstOrDefaultAsync();
        if(userId <= 0) throw new UserNotFoundException($"User does not exist with username: {userName}.");

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
            .AsNoTracking()
            .Where(s => s.Id == storyId)
            .Select(StoryMappers.ProjectToStoryInfoForSessionDto)
            .FirstOrDefaultAsync();
        
        return storyInfo;
    }

    public async Task ChangeCurrentStoryAuthorAsync(int storyId, string userName)
    {
        Story? story = await _context.Story.FirstOrDefaultAsync(s => s.Id == storyId);
        if(story is null) throw new StoryNotFoundException("Story does not exist.");
        
        int authorId = await _context.AppUser
            .Where(au => au.UserName == userName)
            .Select(au => au.Id)
            .FirstOrDefaultAsync();
        if(authorId <= 0) throw new UserNotFoundException($"User does not exist with username: {userName}.");

        story.CurrentAuthorId = authorId;
        await _context.SaveChangesAsync();
    }
    
    public async Task<StoryPartDto?> CreateStoryPartAsync(int storyId, string userName, CreateStoryPartDto storyPartDto)
    {
        StoryPart newStoryPart = storyPartDto.ToStoryPartModel();
        newStoryPart.StoryId = storyId;

        AppUser? creatorUser = await _context.AppUser
            .Where(au => au.UserName == userName)
            .FirstOrDefaultAsync();
        if(creatorUser is null) throw new UserNotFoundException($"User does not exist with username: {userName}.");
        
        AuthorInStory? authorInStory = await _context.AuthorInStory
            .FirstOrDefaultAsync(ais => ais.AuthorId == creatorUser.Id && ais.StoryId == storyId);
        if(authorInStory is null) throw new UserNotInStoryException("User not part of story.");

        int? currentStoryAuthorId = await _context.Story
            .Where(s => s.Id == storyId)
            .Select(s => s.CurrentAuthorId)
            .FirstOrDefaultAsync();
        if(currentStoryAuthorId is null) throw new StoryNotFoundException("Story does not exist.");
        
        if (currentStoryAuthorId != creatorUser.Id) return null;
        
        newStoryPart.UserId = creatorUser.Id;
        await _context.AddAsync(newStoryPart);
        await _context.SaveChangesAsync();
        return newStoryPart.ToStoryPartDto();
    }

    public async Task<PagedKeysetStoryList<StoryPartInListDto>> GetStoryPartsAsync(int storyId, int? lastId = null, int pageSize = 15)
    {
        List<StoryPartInListDto> storyParts = await _context.StoryPart
            .AsNoTracking()
            .Where(sp => sp.StoryId == storyId)
            .OrderByDescending(sp => sp.Id)
            .Where(sp => !lastId.HasValue || sp.Id <= lastId)
            .Take(pageSize + 1)
            .Select(StoryPartMappers.ProjectToStoryPartInListDto)
            .ToListAsync();
        
        bool hasMore = storyParts.Count > pageSize;
        int? nextId = null;
        if (hasMore)
        {
            nextId = storyParts[^1].Id;
            storyParts.RemoveAt(storyParts.Count - 1);
        }

        PagedKeysetStoryList<StoryPartInListDto> pagedStoryParts = new()
        {
            Items = storyParts,
            HasMore = hasMore,
            NextId = nextId
        };

        return pagedStoryParts;
    }

    public async Task<bool> DeleteStoryPart(int storyId, int storyPartId)
    {
        StoryPart? storyPartToDelete = await _context.StoryPart.FirstOrDefaultAsync(storyPart => storyPart.Id == storyPartId);
        
        if(storyPartToDelete is null || storyPartToDelete.StoryId != storyId) return false;

        _context.Remove(storyPartToDelete);
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> IsStoryPartCreator(string userName, int storyPartId)
    {
        int userId =  await _context.AppUser
            .Where(au => au.UserName == userName)
            .Select(au => au.Id)
            .FirstOrDefaultAsync();
        if (userId <= 0) throw new UserNotFoundException($"User does not exist with username: {userName}.");

        bool isStoryPartCreator = await _context.StoryPart
            .Where(s => s.Id == storyPartId)
            .AnyAsync(s => s.UserId == userId);
        return isStoryPartCreator;
    }
}