using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using api.Dtos.AppUser;
using Microsoft.AspNetCore.Authorization;
using api.Constants;
using System.Security.Claims;
using api.Dtos.HttpResponses;
using api.Dtos.Pagination;
using api.Exceptions;
using api.Helpers;
using api.Interfaces;

namespace api.Controllers;

[Authorize(Roles = RoleConstants.User)]
[ApiController]
[Route("accounts")]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly ILogger<AccountController> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;
    
    public AccountController(
        IAccountService accountService, 
        ILogger<AccountController> logger, 
        IDateTimeProvider dateTimeProvider)
    {
        _accountService = accountService;
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
    }

    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(PagedKeysetStoryList<UserMainInfoDto>),StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedKeysetStoryList<UserMainInfoDto>>> GetUsers(
        [FromQuery] DateTimeOffset? lastDate, 
        [FromQuery] string? lastUserName)
    {
        const int pageSize = 15;
        PagedKeysetUserList<UserMainInfoDto> pagedUsers = await _accountService.GetUsersAsync(lastDate, lastUserName, pageSize);
        return Ok(pagedUsers);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(TokenResponse),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TokenResponse>> Register([FromBody] RegisterUserDto registerUser)
    {
        try
        {
            string token = await _accountService.RegisterAsync(registerUser);
            _logger.LogInformation("User '{UserName}' registered at {RegisterTime}", registerUser.UserName, _dateTimeProvider.UtcNow);
            return Ok(new TokenResponse { Token = token });
        }
        catch (UserRegistrationException ex)
        {
            _logger.LogWarning(ex, "User '{UserName}' registration failed at {RegisterTime}", registerUser.UserName, _dateTimeProvider.UtcNow);
            var errors = ex.Errors;
            
            if (errors is null) return ValidationProblem(ModelState);
            ControllerHelpers.AddErrorsToModelState(errors, ModelState);
            
            return ValidationProblem(ModelState);
        }
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenResponse>> Login([FromBody] LoginUserDto loginUser)
    {
        try
        {
            string? token = await _accountService.LoginAsync(loginUser);

            if(token is null) return Unauthorized();
        
            _logger.LogInformation("User '{UserName}' logged in at {LoginTime}", loginUser.UserName, _dateTimeProvider.UtcNow);
            return Ok(new TokenResponse { Token = token });
        }
        catch (UserNotFoundException ex)
        {
            _logger.LogWarning(ex, "User '{UserName}' was not found, login failed at {LoginTime}.", loginUser.UserName, _dateTimeProvider.UtcNow);
            return Unauthorized();
        }
    }
    
    [HttpGet("{username}")]
    [ProducesResponseType(typeof(PublicAppUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PublicAppUserDto>> GetUser([FromRoute] string username)
    {
        PublicAppUserDto? appUser = await _accountService.GetPublicUserAsync(username);

        if(appUser is null) return NotFound();
        
        return Ok(appUser);
    }
    
    [HttpGet("me")]
    [ProducesResponseType(typeof(PrivateAppUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PrivateAppUserDto>> GetLoggedUser()
    {
        string? loggedUser = User.FindFirstValue(ClaimTypes.Name);
        if (loggedUser is null) return Unauthorized();
        
        PrivateAppUserDto? appUser = await _accountService.GetPrivateUserAsync(loggedUser);
        if(appUser is null) return NotFound();
        
        return Ok(appUser);
    }
    
    [HttpPut("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateUser([FromBody] UpdateUserFieldsDto updateUserData)
    {
        try
        {
            string? loggedUser = User.FindFirstValue(ClaimTypes.Name);
            if(loggedUser is null) return Unauthorized();
        
            await _accountService.UpdateUserFieldsAsync(loggedUser, updateUserData);
            return Ok();
        }
        catch (UserUpdateException ex)
        {
            if (ex.Errors is null) return ValidationProblem(ModelState);
            ControllerHelpers.AddErrorsToModelState(ex.Errors, ModelState);
            return ValidationProblem(ModelState);
        }
    }

    [HttpPut("me/password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        string? loggedUser = User.FindFirstValue(ClaimTypes.Name);
        if(loggedUser is null) return Unauthorized();

        try
        {
            await _accountService.ChangeUserPasswordAsync(loggedUser, changePasswordDto);
            return Ok();
        }
        catch (UserUpdateException ex)
        {
            if (ex.Errors is null) return ValidationProblem(ModelState);
            ControllerHelpers.AddErrorsToModelState(ex.Errors, ModelState);

            _logger.LogWarning(ex, "User '{UserName}' password change failed at {ChangeTime}.", loggedUser, _dateTimeProvider.UtcNow);
            return ValidationProblem(ModelState);
        }
        catch (UserNotFoundException ex)
        {
            _logger.LogWarning(
                ex,
                "User was not found '{UserName}', password change failed at {ChangeTime}.", 
                loggedUser, 
                _dateTimeProvider.UtcNow);
            return Unauthorized();
        }
    }

    [Authorize(Policy = PolicyConstants.RequiredAdminRole)]
    [HttpDelete("{username}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteUser([FromRoute] string username)
    {
        try
        {
            await _accountService.DeleteByNameAsync(username);
            return Ok();
        }
        catch (UserNotFoundException ex)
        {
            _logger.LogWarning(ex, "User '{UserName}' deletion failed at {DeleteTime}", username, _dateTimeProvider.UtcNow);
            return NotFound();
        }
    }

    [HttpPut("me/profile-image")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Consumes(MediaTypeNames.Multipart.FormData)]
    public async Task<ActionResult> UpdateProfileImage(IFormFile image)
    {
        string? loggedUser = User.FindFirstValue(ClaimTypes.Name);
        if(loggedUser is null)
        {
            return Unauthorized();
        }

        await _accountService.UpdateProfileImageAsync(loggedUser, image, DirectoryPathConstants.ProfileImages);

        return Ok();
    }
    
    [HttpDelete("me/profile-image")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteProfileImage()
    {
        string? loggedUser = User.FindFirstValue(ClaimTypes.Name);

        if(loggedUser is null)
        {
            return Unauthorized();
        }

        await _accountService.DeleteProfileImageAsync(loggedUser, DirectoryPathConstants.ProfileImages);

        return Ok();
    }
}