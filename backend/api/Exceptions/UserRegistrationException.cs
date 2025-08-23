using Microsoft.AspNetCore.Identity;

namespace api.Exceptions;

public class UserRegistrationException : Exception
{
    public IReadOnlyDictionary<string, string[]>? Errors { get; }
    
    public UserRegistrationException()
    {
    }
    
    public UserRegistrationException(string message) : base(message)
    {
    }

    public UserRegistrationException(string message, Exception innerException) : base(message, innerException)
    {
    }
    
    public UserRegistrationException(string message, IReadOnlyCollection<IdentityError> identityErrors) : base(message)
    {
        Errors = BuildErrors(identityErrors);
    }
    
    public UserRegistrationException(string message, IReadOnlyCollection<IdentityError> identityErrors, Exception innerException) : base(message, innerException)
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