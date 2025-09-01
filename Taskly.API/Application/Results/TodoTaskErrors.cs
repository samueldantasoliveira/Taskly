namespace Taskly.Application.Results;

public static class TodoTaskErrors
{
    public static readonly Error InvalidTitle =
        Error.Create("TodoTask.InvalidTitle", "Title is invalid");

    public static readonly Error NotFound =
        Error.Create("TodoTask.NotFound", "TodoTask Not Found");

    public static readonly Error NoChangesDetected =
        Error.Create("TodoTask.NoChangesDetected", "No Changes Detected on the TodoTask");
}