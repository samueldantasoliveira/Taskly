namespace Taskly.Application.Results;

public enum AddProjectFailureReason
{
    None,
    TeamNotFound,
    TeamInactive,
    InvalidName
}