namespace Taskly.Application.Results;

public sealed class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException() : base("This record changed. Reload it before trying again.") { }

    public static void Check(long? expected, long actual)
    {
        if (expected.HasValue && expected.Value != actual)
            throw new ConcurrencyConflictException();
    }
}
