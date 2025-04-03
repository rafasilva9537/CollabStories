using System.Security.Claims;
using System.Text.RegularExpressions;
using api.Dtos.StoryPart;
using api.Models;
using api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.VisualBasic;

namespace api.Hubs;

public interface IStoryClient
{
    Task ReceiveStoryPart(string username, string storyPartText);
    Task MessageFailed(string message);
    Task UserDisconnected(string? username);
    Task UserConnected(string? username);
}

[Authorize]
public class StoryHub : Hub<IStoryClient>
{
    private readonly IStoryService _storyService;
    public StoryHub(IStoryService storyService)
    {
        _storyService = storyService;
    }

    public async Task JoinStorySession(int storyId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, storyId.ToString());
        await Clients.Group(storyId.ToString()).UserConnected(Context.User?.Identity?.Name);
    }

    public async Task LeaveStorySession(int storyId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, storyId.ToString());
        await Clients.Group(storyId.ToString()).UserDisconnected(Context.User?.Identity?.Name);
    }


    public async Task SendStoryPart(int storyId, string storyPartText)
    {     
        var newStoryPart = new CreateStoryPartDto
        {
            Text = storyPartText
        };

        string? userName = Context.User?.Identity?.Name;

        if(userName is null)
        {
            await Clients.Caller.MessageFailed("Unable to send message. It wasn't possible to get logged user.");
            return;
        }

        StoryPartDto? createdStoryPart = await _storyService.CreateStoryPartAsync(storyId, userName, newStoryPart);

        if(createdStoryPart is null)
        {
            await Clients.Caller.MessageFailed("Unable to send message. User isn't in story.");
            return;
        }

        await Clients.Group(storyId.ToString()).ReceiveStoryPart(userName, createdStoryPart.Text);
    }

    public async Task OnConnectedAsync(int storyId)
    {
        // TODO: change all to group
        
        await Clients.All.UserConnected(Context.User?.Identity?.Name);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // TODO: change all to group
        await Clients.All.UserConnected(Context.User?.Identity?.Name);
        await base.OnDisconnectedAsync(exception);
    }
}