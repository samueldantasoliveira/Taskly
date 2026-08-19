namespace Taskly.Application.Results;

public static class TodoTaskErrors
{
    public static readonly Error ProjectNotFound =
        Error.Create("TodoTask.ProjectNotFound", "Project not found");
    
    public static readonly Error ProjectInactive =
        Error.Create("TodoTask.ProjectInactive", "Project is inactive");
    
    public static readonly Error UserNotFound =
        Error.Create("TodoTask.UserNotFound", "User not found");
    
    public static readonly Error InvalidTitle =
        Error.Create("TodoTask.InvalidTitle", "Title is invalid");

    public static readonly Error NotFound =
        Error.Create("TodoTask.NotFound", "TodoTask not found");

    public static readonly Error NoChangesDetected =
        Error.Create("TodoTask.NoChangesDetected", "No changes detected on the todoTask");

    public static readonly Error TeamNotFound =
        Error.Create("TodoTask.TeamNotFound", "Team not found.");

    public static readonly Error TeamInactive =
        Error.Create("TodoTask.TeamInactive", "Team is inactive.");

    public static readonly Error UserNotTeamMember =
        Error.Create("TodoTask.UserNotTeamMember", "User is not a member of the project team.");

    public static readonly Error AssignedUserNotTeamMember =
        Error.Create("TodoTask.AssignedUserNotTeamMember", "Assigned user is not a member of the project team.");

    public static readonly Error NotAssignedUser = 
        Error.Create("TodoTask.NotAssignedUser", "You are not the user assigned to this task.");
}