namespace Taskly.Application.Results;

public static class TeamErrors
{
    public readonly static Error InvalidName =
        Error.Create("Team.InvalidName", "Invalid name");
    public readonly static Error NotFound =
        Error.Create("Team.NotFound", "Team not found");

    public readonly static Error Inactive =
        Error.Create("Team.Inactive", "Team is inactive");
    public readonly static Error UserNotFound =
        Error.Create("Team.UserNotFound", "User not found");

    public readonly static Error UserAlreadyMember =
        Error.Create("Team.UserAlreadyMember", "User is already a member");
    public readonly static Error UserNotMember =
        Error.Create("Team.UserNotMember", "User is not a member");
    public readonly static Error NotOwner =
        Error.Create("Team.NotOwner", "Only the team owner can perform this action");
    public readonly static Error NotAuthorized =
        Error.Create("Team.NotAuthorized", "User is not a member of this team");
    public readonly static Error OwnerCannotBeRemoved =
        Error.Create("Team.OwnerCannotBeRemoved", "Transfer ownership before removing the team owner.");
}
