namespace api.Exceptions;

public class UserNotInStoryException : Exception
{
    public UserNotInStoryException() : base()
    {
    }

    public UserNotInStoryException(string message) : base(message)
    {
    }

    public UserNotInStoryException(string message, Exception innerException) : base(message, innerException)
    {
    }
}