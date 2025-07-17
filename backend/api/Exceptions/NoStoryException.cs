namespace api.Exceptions;

public class NoStoryException : Exception
{
    public NoStoryException()
    {
    }

    public NoStoryException(string message) : base(message)
    {
    }
    
    public NoStoryException(string message, Exception innerException) : base(message, innerException)
    {
    }
}