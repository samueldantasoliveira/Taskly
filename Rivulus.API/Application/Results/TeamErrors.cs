namespace Rivulus.Application.Results;

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
    public readonly static Error InvalidInvitationEmail = Error.Create("Team.InvalidInvitationEmail", "Invitation email is invalid.");
    public readonly static Error InvitationAlreadyPending = Error.Create("Team.InvitationAlreadyPending", "An invitation is already pending for this email.");
    public readonly static Error InvitationNotFound = Error.Create("Team.InvitationNotFound", "Invitation not found.");
    public readonly static Error InvitationExpired = Error.Create("Team.InvitationExpired", "Invitation has expired or is no longer available.");
    public readonly static Error InvitationEmailMismatch = Error.Create("Team.InvitationEmailMismatch", "This invitation belongs to another email address.");
}
