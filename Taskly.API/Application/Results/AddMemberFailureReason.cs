namespace Taskly.Application.Results;

public enum AddMemberFailureReason
{
    None,
    TeamNotFound,
    TeamInactive,
    UserNotFound,
    UserInactive,
    UserAlreadyMember
}