using Microsoft.AspNetCore.Identity;

namespace api.Exceptions;

public class UserUpdateException : IdentityErrorsBaseException
{
    public UserUpdateException()
    {
    }
    
    public UserUpdateException(string message) : base(message)
    {
    }

    public UserUpdateException(string message, Exception innerException) : base(message, innerException)
    {
    }
    
    public UserUpdateException(
        string message, 
        IReadOnlyCollection<IdentityError> identityErrors) : base(message, identityErrors)
    {
    }
    
    public UserUpdateException(
        string message, 
        IReadOnlyCollection<IdentityError> identityErrors, 
        Exception innerException) : base(message, identityErrors, innerException)
    {
    }
}