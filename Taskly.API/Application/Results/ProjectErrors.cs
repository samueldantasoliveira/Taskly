namespace Taskly.Application.Results;

public static class ProjectErrors
{
    public static readonly Error NotFound =
        Error.Create("Project.NotFound", "Project not found");
    public static readonly Error InvalidName =
        Error.Create("Project.InvalidName", "Project Name is Invalid");
    public static readonly Error InvalidDescription =
        Error.Create("Project.InvalidDescription", "Project description is invalid");
    public static readonly Error InvalidStatus =
        Error.Create("Project.InvalidStatus", "Project status is invalid");
    public static readonly Error Inactive =
        Error.Create("Project.Inactive", "Project is inactive");
    public static readonly Error OwnerNotFound =
        Error.Create("Project.OwnerNotFound", "Owner not found");
    public static readonly Error TeamNotFound =
        Error.Create("Project.TeamNotFound", "Team not found");
    public static readonly Error TeamInactive =
        Error.Create("Project.TeamInactive", "Team is inactive");
    public static readonly Error UserNotTeamMember =
        Error.Create("Project.UserNotTeamMember", "User is not a member of the team.");
    public static readonly Error NotAuthorized =
        Error.Create("Project.NotAuthorized", "User is not authorized to manage this project.");
    public static readonly Error OwnerNotInDestinationTeam =
        Error.Create("Project.OwnerNotInDestinationTeam", "Transfer ownership to a member of the destination team before moving the project.");
    public static readonly Error AssigneesNotInDestinationTeam =
        Error.Create("Project.AssigneesNotInDestinationTeam", "Reassign or unassign all task assignees before moving the project.");
}
