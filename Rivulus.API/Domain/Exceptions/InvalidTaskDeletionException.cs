namespace Rivulus.Domain.Exceptions;

public class InvalidTaskDeletionException : Exception
{
    public InvalidTaskDeletionException(string message)
        : base(message)
    {
    }
}