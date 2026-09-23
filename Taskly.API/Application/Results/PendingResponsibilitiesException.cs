namespace Taskly.Application.Results;

public class PendingResponsibilitiesException() : Exception(
    "Transfer or delete your teams and projects, and reassign your active tasks before deleting your account.");
