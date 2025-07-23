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
            AddMemberFailureReason.TeamInactive => "Team is inactive",
            AddMemberFailureReason.TeamNotFound => "Team not found",
            AddMemberFailureReason.UserInactive => "User is inactive",
            AddMemberFailureReason.UserNotFound => "User not found",
            AddMemberFailureReason.UserAlreadyMember => "User is already a member of the team",
            AddTeamFailureReason.InvalidName => "User name is invalid",
            AddUserFailureReason.InvalidName => "User name is invalid",
            _ => "Unknow error."
        };
    }
}