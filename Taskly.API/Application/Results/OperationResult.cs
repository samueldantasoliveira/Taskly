public class OperationResult<TFailureReason>
    where TFailureReason : struct, Enum
{
    public bool Success => EqualityComparer<TFailureReason>.Default.Equals(FailureReason, default);
    public TFailureReason FailureReason { get; init; } = default;
    public static OperationResult<TFailureReason> Fail(TFailureReason reason) =>
    new() { FailureReason = reason };
    public static OperationResult<TFailureReason> Ok() => new();

}
