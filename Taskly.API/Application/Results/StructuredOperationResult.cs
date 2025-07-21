using Taskly.Application.Results;

public class StructuredOperationResult<T>
{
    public bool Success { get; init; }
    public T? Value { get; init; }
    public Error? Error { get; init; }

    public static StructuredOperationResult<T> Ok(T value) => new() {Success = true, Value = value};
    public static StructuredOperationResult<T> Fail(Error error)
    {
        if (error is null) throw new ArgumentNullException(nameof(error));
        return new() {Success = false, Error = error};
    } 
    
}