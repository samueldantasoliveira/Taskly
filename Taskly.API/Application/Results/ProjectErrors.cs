namespace Taskly.Application.Results;

public static class ProjectErrors
{
    public static readonly Error NotFound =
        Error.Create("Project.NotFound", "Project not found");
    public static readonly Error InvalidName =
        Error.Create("Project.InvalidName", "Project Name is Invalid");
    public static readonly Error Inactive =
        Error.Create("Project.Inactive", "Project is inactive");
    public static readonly Error OwnerNotFound =
        Error.Create("Project.OwnerNotFound", "Owner not found");
    public static readonly Error TeamNotFound =
        Error.Create("Project.TeamNotFound", "Team not found");
    public static readonly Error TeamInactive =
        Error.Create("Project.TeamInactive", "Team is inactive");
        
}