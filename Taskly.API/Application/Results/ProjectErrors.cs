namespace Taskly.Application.Results;

public static class ProjectErrors
{
    public static readonly Error NotFound =
        Error.Create("Project.NotFound", "Project not found");

    public static readonly Error Inactive =
        Error.Create("Project.Inactive", "Project is inactive");
}