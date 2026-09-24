namespace Rivulus.Domain.Exceptions;

public class InvalidTaskTransitionException : Exception
{
    public InvalidTaskTransitionException(string message)
        : base(message)
    {
    }
}