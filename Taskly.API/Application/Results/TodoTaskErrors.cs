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
}