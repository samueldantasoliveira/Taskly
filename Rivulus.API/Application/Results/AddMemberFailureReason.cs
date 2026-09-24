namespace Rivulus.Application.Results;

public enum AddMemberFailureReason
{
    None,
    TeamNotFound,
    TeamInactive,
    UserNotFound,
    UserInactive,
    UserAlreadyMember
}