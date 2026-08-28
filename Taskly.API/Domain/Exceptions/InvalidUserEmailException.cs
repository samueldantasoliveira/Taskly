namespace Taskly.Domain.Exceptions;

public class InvalidUserEmailException : Exception
{
    public InvalidUserEmailException(string message)
        : base(message)
    {
    }
}