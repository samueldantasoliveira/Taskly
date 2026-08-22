namespace Taskly.Domain.Exceptions;

public class OwnerCannotBeRemovedException : Exception
{
    public OwnerCannotBeRemovedException(string message)
        : base(message)
    {
    }
}