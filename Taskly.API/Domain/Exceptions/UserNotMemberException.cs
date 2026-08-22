namespace Taskly.Domain.Exceptions;

public class UserNotMemberException : Exception
{
    public UserNotMemberException(string message)
        : base(message)
    {
    }
}