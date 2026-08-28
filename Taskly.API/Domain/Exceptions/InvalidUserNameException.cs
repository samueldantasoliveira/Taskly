namespace Taskly.Domain.Exceptions;

public class InvalidUserNameException : Exception
{
    public InvalidUserNameException(string message)
        : base(message)
    {
    }
}