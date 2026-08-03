using Taskly.Application.Results;

public class StructuredOperationResult
{
    public bool Success { get; init; }
    public Error? Error { get; init; }

    public static StructuredOperationResult Ok() => new() {Success = true};

    public static StructuredOperationResult Fail(Error error)
    {
        if (error is null) throw new ArgumentNullException(nameof(error));
        return new() {Success = false, Error = error};
    } 
}
public class StructuredOperationResult<T> : StructuredOperationResult
{
    public T? Value { get; init; }

    public static StructuredOperationResult<T> Ok(T value) => new() {Success = true, Value = value};
    public new static StructuredOperationResult<T> Fail(Error error)
    {
        if (error is null) throw new ArgumentNullException(nameof(error));
        return new() {Success = false, Error = error};
    } 
    
}