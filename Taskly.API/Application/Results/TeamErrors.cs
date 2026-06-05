namespace Taskly.Application.Results;

public static class TeamErrors
{
    public readonly static Error InvalidName =
        Error.Create("Team.InvalidName", "Invalid name");
    public readonly static Error NotFound =
        Error.Create("Team.NotFound", "Team not found");

    public readonly static Error Inactive =
        Error.Create("Team.Inactive", "Team is inactive");

    public readonly static Error UserAlreadyMember =
        Error.Create("Team.UserAlreadyMember", "User is already a member");
    public readonly static Error UserNotMember =
        Error.Create("Team.UserNotMember", "User is not a member");
}