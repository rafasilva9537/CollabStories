using Microsoft.AspNetCore.Identity;

namespace api.Exceptions;

public class UserPasswordException : IdentityErrorsBaseException
{
    public UserPasswordException()
    {
    }
    
    public UserPasswordException(string message) : base(message)
    {
    }

    public UserPasswordException(string message, Exception innerException) : base(message, innerException)
    {
    }
    
    public UserPasswordException(string message, IReadOnlyCollection<IdentityError> identityErrors) : base(message)
    {
    }
    
    public UserPasswordException(string message, IReadOnlyCollection<IdentityError> identityErrors, Exception innerException) : base(message, innerException)
    {
    }
}