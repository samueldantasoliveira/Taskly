namespace Rivulus.Domain.Exceptions;

public class InvalidTaskUpdateException : Exception
{
    public InvalidTaskUpdateException(string message)
        : base(message)
    {
    }
}