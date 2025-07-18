using api.Dtos.Story;
using api.Dtos.StoryPart;

namespace api.Interfaces;

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
    Task<StoryInfoForSessionDto?> GetStoryInfoForSessionAsync(int storyId);
    Task ChangeCurrentStoryAuthor(int storyId, string username);

    Task<StoryPartDto?> CreateStoryPartAsync(int storyId, string username, CreateStoryPartDto storyPartDto);
    Task<bool> DeleteStoryPart(int storyId, int storyPartId);
    Task<bool> IsStoryPartCreator(string username, int storyPartId);
}