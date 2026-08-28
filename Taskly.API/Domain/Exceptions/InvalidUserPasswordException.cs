namespace Taskly.Domain.Exceptions;

public class InvalidUserPasswordException : Exception
{
    public InvalidUserPasswordException(string message)
        : base(message)
    {
    }
}