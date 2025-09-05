using api.Dtos.Pagination;
using api.Dtos.Story;
using api.Dtos.StoryPart;
using api.Models;

namespace api.Interfaces;

public interface IStoryService
{
    Task<PagedKeysetStoryList<StoryMainInfoDto>> GetStoriesAsync(int? lastId = null, int pageSize = 15);
    
    Task<StoryDto?> GetStoryAsync(int id);
    
    Task<StoryDto> CreateStoryAsync(CreateStoryDto createStoryDto, string userName);
    
    /// <summary>
    /// Deletes a story by its id.
    /// </summary>
    /// <param name="id">The identifier of the story to be deleted.</param>
    /// <returns>The task that has the boolean result indicating whether the story was successfully deleted.</returns>
    /// <remarks>The story doesn't exist if that's the case.</remarks>
    Task<bool> DeleteStoryAsync(int id);
    
    // TODO: possible addition of logged user parameter when service to controller validation is implemented
    Task<StoryDto> UpdateStoryAsync(int storyId, UpdateStoryDto updateStoryDto);
    
    Task<bool> IsStoryOwner(string username, int storyId);
    
    Task<string> ChangeToNextCurrentAuthorAsync(int storyId);
    
    Task<string> GetCurrentAuthorUserNameAsync(int storyId);
    
    Task ChangeCurrentStoryAuthorAsync(int storyId, string username);
    
    Task<CompleteStoryDto?> GetCompleteStoryAsync(int storyId);
    
    Task<bool> JoinStoryAsync(string username, int storyId);
    
    Task<bool> LeaveStoryAsync(string username, int storyId);
    
    Task<bool> IsStoryAuthorAsync(string username, int storyId);
    
    Task<bool> StoryExistsAsync(int storyId);
    
    Task<StoryInfoForSessionDto?> GetStoryInfoForSessionAsync(int storyId);

    Task<StoryPartDto?> CreateStoryPartAsync(int storyId, string username, CreateStoryPartDto storyPartDto);
    
    Task<bool> DeleteStoryPart(int storyId, int storyPartId);
    
    Task<bool> IsStoryPartCreator(string username, int storyPartId);
}