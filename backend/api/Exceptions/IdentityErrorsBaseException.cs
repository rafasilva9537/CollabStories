using Microsoft.AspNetCore.Identity;

namespace api.Exceptions;

/// <summary>
/// Provides a base exception for handling identity-related errors that involve 
/// <see cref="Microsoft.AspNetCore.Identity.IdentityError"/> instances.
/// This exception can be used, for example, when handling errors during user registration 
/// or password reset operations performed with <see cref="Microsoft.AspNetCore.Identity.UserManager{TUser}"/>.
/// It allows mapping identity errors to the appropriate fields in controllers 
/// so they can be returned to the client in a structured way.
/// </summary>
public abstract class IdentityErrorsBaseException : Exception
{
    public IReadOnlyDictionary<string, string[]>? Errors { get; }
    
    protected IdentityErrorsBaseException()
    {
    }

    protected IdentityErrorsBaseException(string message) : base(message)
    {
    }

    protected IdentityErrorsBaseException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected IdentityErrorsBaseException(string message, IReadOnlyCollection<IdentityError> identityErrors) : base(message)
    {
        Errors = BuildErrors(identityErrors);
    }

    protected IdentityErrorsBaseException(string message, IReadOnlyCollection<IdentityError> identityErrors,
        Exception innerException) : base(message, innerException)
    {
        Errors = BuildErrors(identityErrors);
    }

    private static Dictionary<string, string[]> BuildErrors(IReadOnlyCollection<IdentityError> identityErrors)
    {
        var errors = identityErrors.GroupBy(e => e.Code, e => e.Description)
            .ToDictionary(
                group => group.Key,
                group => group.ToArray()
            );
        return errors;
    }
}