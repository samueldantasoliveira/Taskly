namespace Taskly.Domain.Exceptions;

public class UserAlreadyMemberException : Exception
{
    public UserAlreadyMemberException(string message)
        : base(message)
    {
    }
}