using Taskly.Application.Results;

public class StructuredOperationResult<T>
{
    public bool Success { get; init; }
    public T? Value { get; init; }
    public Error? Error { get; init; }

    public static StructuredOperationResult<T> Ok(T value) => new() {Success = true, Value = value};
    public static StructuredOperationResult<T> Fail(Error error) => new() {Success = false, Error = error};
    
}