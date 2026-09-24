namespace Rivulus.Application.Results;

public static class TodoTaskErrors
{
    public static readonly Error InvalidComment = Error.Create("TodoTask.InvalidComment", "Comment must contain between 1 and 1000 characters.");
    public static readonly Error CommentNotFound = Error.Create("TodoTask.CommentNotFound", "Comment not found.");
    public static readonly Error NotCommentAuthor = Error.Create("TodoTask.NotCommentAuthor", "Only the comment author can change it.");
    public static readonly Error ProjectNotFound =
        Error.Create(
            "TodoTask.ProjectNotFound", 
            "Project not found");
    
    public static readonly Error ProjectInactive =
        Error.Create(
            "TodoTask.ProjectInactive", 
            "Project is inactive");
    
    public static readonly Error UserNotFound =
        Error.Create(
            "TodoTask.UserNotFound", 
            "User not found");
    
    public static readonly Error InvalidTitle =
        Error.Create(
            "TodoTask.InvalidTitle", 
            "Title is invalid");
    public static readonly Error InvalidPriority =
        Error.Create("TodoTask.InvalidPriority", "Task priority is invalid.");

    public static readonly Error NotFound =
        Error.Create(
            "TodoTask.NotFound", 
            "TodoTask not found");

    public static readonly Error NoChangesDetected =
        Error.Create(
            "TodoTask.NoChangesDetected", 
            "No changes detected on the todoTask");

    public static readonly Error TeamNotFound =
        Error.Create(
            "TodoTask.TeamNotFound", 
            "Team not found.");

    public static readonly Error TeamInactive =
        Error.Create(
            "TodoTask.TeamInactive", 
            "Team is inactive.");

    public static readonly Error UserNotTeamMember =
        Error.Create(
            "TodoTask.UserNotTeamMember", 
            "User is not a member of the project team.");

    public static readonly Error AssignedUserNotTeamMember =
        Error.Create(
            "TodoTask.AssignedUserNotTeamMember", 
            "Assigned user is not a member of the project team.");

    public static readonly Error NotAssignedUser = 
        Error.Create(
            "TodoTask.NotAssignedUser",
            "You are not the user assigned to this task.");
    
    public static readonly Error InvalidPage =
        Error.Create(
          "TodoTask.InvalidPage",
          "Page must be greater than or equal to 1.");

    public static readonly Error InvalidPageSize =
        Error.Create(
          "TodoTask.InvalidPageSize",
          "Page size must be between 1 and 100.");

    public static readonly Error PaginationLimitExceeded =
        Error.Create(
          "TodoTask.PaginationLimitExceeded",
          "The requested page exceeds the supported pagination limit.");
    public static readonly Error InvalidStatusFilter =
      Error.Create(
          "TodoTask.InvalidStatusFilter",
          "Status filter is invalid.");

    public static readonly Error InvalidSortBy =
        Error.Create(
            "TodoTask.InvalidSortBy",
            "Sort field is invalid.");

    public static readonly Error InvalidSortDirection =
        Error.Create(
            "TodoTask.InvalidSortDirection",
            "Sort direction is invalid.");


}
