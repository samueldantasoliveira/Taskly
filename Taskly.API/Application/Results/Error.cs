namespace Taskly.Application.Results;

public class Error
{
    public required string Code { get; init; }
    public required string Message { get; init; }

    public static Error FromEnum(Enum e) =>
        new() { Code = e.ToString(), Message = FormatMessage(e) };

    public static string FormatMessage(Enum e)
    {
        return e switch
        {
            AddUserFailureReason.InvalidName => "User name is invalid",
            _ => "Unknow error."
        };
    }
}