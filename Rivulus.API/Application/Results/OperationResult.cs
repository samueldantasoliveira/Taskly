public class OperationResult<TFailureReason>
    where TFailureReason : struct, Enum
{
    public bool Success => EqualityComparer<TFailureReason>.Default.Equals(FailureReason, default);
    public TFailureReason FailureReason { get; init; } = default;
    public static OperationResult<TFailureReason> Fail(TFailureReason reason) =>
    new() { FailureReason = reason };
    public static OperationResult<TFailureReason> Ok() => new();
}
public class OperationResult<TFailureReason, TValue>
    where TFailureReason : struct, Enum
{
    public bool Success => EqualityComparer<TFailureReason>.Default.Equals(FailureReason, default);
    public TFailureReason FailureReason { get; init; } = default;
    public TValue? Value { get; init; }

    public static OperationResult<TFailureReason, TValue> Fail(TFailureReason reason) =>
    new() { FailureReason = reason };
    public static OperationResult<TFailureReason, TValue> Ok(TValue value) =>
    new() { Value = value };

}
