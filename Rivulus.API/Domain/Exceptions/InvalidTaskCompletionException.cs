namespace Rivulus.Domain.Exceptions;

public class InvalidTaskCompletionException : Exception
{
    public InvalidTaskCompletionException(string message)
        : base(message)
    {
    }
}