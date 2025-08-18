namespace api.Exceptions;

public class StoryNotFoundException : Exception
{
    public StoryNotFoundException()
    {
    }

    public StoryNotFoundException(string message) : base(message)
    {
    }
    
    public StoryNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}