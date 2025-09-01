namespace Taskly.Application.Results;

public static class UserErrors
{
    public readonly static Error NotFound =
        Error.Create("User.NotFound", "User not found");

    public readonly static Error Inactive =
        Error.Create("User.Inactive", "User is inactive");
}