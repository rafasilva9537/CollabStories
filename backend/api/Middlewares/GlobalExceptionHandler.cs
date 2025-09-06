using api.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace api.Middlewares;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;
    
    public GlobalExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }
    
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, 
        Exception exception, 
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception of type '{ExceptionType}' occurred.", exception.GetType().Name);

        int status = exception switch
        {
            StoryNotFoundException => StatusCodes.Status404NotFound,
            UserNotFoundException => StatusCodes.Status404NotFound,
            UserRegistrationException => StatusCodes.Status400BadRequest,
            UserUpdateException => StatusCodes.Status400BadRequest,
            UserNotInStoryException => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError,
        };
        
        ProblemDetails problemDetails = new()
        {
            Status = status,
        };
        httpContext.Response.StatusCode = status;

        bool wrote = await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception,
        });

        return wrote;
    }
}