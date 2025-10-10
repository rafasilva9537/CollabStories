using api.Dtos.Pagination;
using api.Dtos.Story;
using api.Dtos.StoryPart;
using api.Exceptions;
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
    
    /// <summary>
    /// Updates the details of an existing story.
    /// </summary>
    /// <param name="storyId">The ID of the story to be updated.</param>
    /// <param name="updateStoryDto">The data transfer object containing the updated properties of the story.</param>
    /// <returns>A <see cref="StoryDto"/> representing the updated story.</returns>
    /// <exception cref="StoryNotFoundException">Thrown when the story with the given ID does not exist.</exception>
    Task<StoryDto> UpdateStoryAsync(int storyId, UpdateStoryDto updateStoryDto);
    
    Task<bool> IsStoryOwner(string userName, int storyId);
    
    Task<string> ChangeToNextCurrentAuthorAsync(int storyId, int turnChanges = 1);
    
    Task<string?> GetCurrentAuthorUserNameAsync(int storyId);
    
    Task ChangeCurrentStoryAuthorAsync(int storyId, string userName);
    
    Task<CompleteStoryDto?> GetCompleteStoryAsync(int storyId);
    
    /// <summary>
    /// Allows a user to join a story, registering them as an author for the specified story.
    /// </summary>
    /// <param name="userName">The username of the user attempting to join the story.</param>
    /// <param name="storyId">The id of the story the user wants to join.</param>
    /// <returns>
    /// Returns true if the user successfully joined the story, otherwise, false if the user was already part of the story.
    /// </returns>
    /// <exception cref="UserNotFoundException">Thrown when the specified user does not exist.</exception>
    /// <exception cref="StoryNotFoundException">Thrown when the specified story does not exist.</exception>
    Task<bool> JoinStoryAsync(string userName, int storyId);
    
    /// <summary>
    /// Allows a user to leave a story, removing them as an author of the specified story.
    /// </summary>
    /// <param name="userName">The username of the user attempting to leave the story.</param>
    /// <param name="storyId">The ID of the story the user wants to leave.</param>
    /// <returns>
    /// Returns true if the user successfully left the story; otherwise, false if the user was not part of the story.
    /// </returns>
    /// <exception cref="UserNotFoundException">Thrown when the specified user does not exist.</exception>
    /// <exception cref="StoryNotFoundException">Thrown when the specified story does not exist.</exception>
    Task<bool> LeaveStoryAsync(string userName, int storyId);
    
    Task<bool> IsStoryAuthorAsync(string userName, int storyId);
    
    Task<bool> StoryExistsAsync(int storyId);
    
    Task<StoryInfoForSessionDto?> GetStoryInfoForSessionAsync(int storyId);

    /// <summary>
    /// Creates a new part for a specified story, authored by the specified user.
    /// </summary>
    /// <param name="storyId">The ID of the story to which the part belongs.</param>
    /// <param name="userName">The username of the user creating the story part.</param>
    /// <param name="storyPartDto">The dto containing the content of the story part to be created.</param>
    /// <returns>
    /// A <see cref="StoryPartDto"/> representing the created story part, or null if the user is not the current author for the story.
    /// </returns>
    /// <exception cref="UserNotFoundException">Thrown when the specified user does not exist.</exception>
    /// <exception cref="UserNotInStoryException">Thrown when the user is not part of the specified story.</exception>
    /// <exception cref="StoryNotFoundException">Thrown when the specified story does not exist.</exception>
    Task<StoryPartDto?> CreateStoryPartAsync(int storyId, string userName, CreateStoryPartDto storyPartDto);
    
    Task<PagedKeysetStoryList<StoryPartInListDto>> GetStoryPartsAsync(int storyId, int? lastId = null, int pageSize = 15);
    
    Task<bool> DeleteStoryPart(int storyId, int storyPartId);
    
    Task<bool> IsStoryPartCreator(string userName, int storyPartId);
}