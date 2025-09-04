using Microsoft.AspNetCore.Identity;

namespace api.Exceptions;

public class UserRegistrationException : IdentityErrorsBaseException
{
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
    }
    
    public UserRegistrationException(string message, IReadOnlyCollection<IdentityError> identityErrors, Exception innerException) : base(message, innerException)
    {
    }
}