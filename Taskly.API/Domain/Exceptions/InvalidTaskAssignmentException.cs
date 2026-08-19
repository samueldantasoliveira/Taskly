namespace Taskly.Domain.Exceptions;

public class InvalidTaskAssignmentException : Exception
{
    public InvalidTaskAssignmentException(string message)
        : base(message)
    {
    }
}