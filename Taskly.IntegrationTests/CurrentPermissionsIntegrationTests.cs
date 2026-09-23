using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Taskly.Application.DTOs;
using Taskly.Domain;
using Taskly.Infrastructure;

namespace Taskly.IntegrationTests;

public class CurrentPermissionsIntegrationTests : IClassFixture<TasklyApiFactory>
{
    private readonly TasklyApiFactory _factory;
    private readonly HttpClient _client;

    public CurrentPermissionsIntegrationTests(TasklyApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public static TheoryData<string, string, HttpStatusCode> BlockedTransitions => BuildCases(
        ["start", "complete", "cancel"],
        [
            ("removed", HttpStatusCode.Forbidden),
            ("left", HttpStatusCode.Forbidden),
            ("team-inactive", HttpStatusCode.BadRequest),
            ("team-deleted", HttpStatusCode.NotFound),
            ("project-inactive", HttpStatusCode.BadRequest),
            ("project-deleted", HttpStatusCode.NotFound),
            ("reassigned", HttpStatusCode.Forbidden)
        ]);

    public static TheoryData<string, string, HttpStatusCode> BlockedProjectChanges => BuildCases(
        ["update", "delete", "move"],
        [
            ("removed", HttpStatusCode.Forbidden),
            ("left", HttpStatusCode.Forbidden),
            ("team-inactive", HttpStatusCode.BadRequest),
            ("team-deleted", HttpStatusCode.NotFound)
        ]);

    [Theory]
    [MemberData(nameof(BlockedTransitions))]
    public async Task TaskTransition_AfterAccessChanges_IsRejectedWithoutChangingTask(
        string action, string change, HttpStatusCode expectedStatus)
    {
        var context = await CreateContextAsync();
        SetToken(context.Member.Token);
        if (action == "complete")
            (await _client.PostAsync($"/api/todotask/{context.Task.Id}/start", null))
                .EnsureSuccessStatusCode();

        await ChangeAccessAsync(context, change);
        using var scope = _factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
        var before = await database.TodoTasks.Find(t => t.Id == context.Task.Id).SingleAsync();

        // Reutiliza o token emitido antes da remoção/saída/inativação.
        SetToken(context.Member.Token);
        var response = await _client.PostAsync($"/api/todotask/{context.Task.Id}/{action}", null);

        Assert.Equal(expectedStatus, response.StatusCode);
        var after = await database.TodoTasks.Find(t => t.Id == context.Task.Id).SingleAsync();
        Assert.Equal(before.Status, after.Status);
        Assert.Equal(before.AssignedUserId, after.AssignedUserId);
        Assert.Equal(before.UpdatedAt, after.UpdatedAt);
    }

    [Theory]
    [MemberData(nameof(BlockedProjectChanges))]
    public async Task ProjectOwner_AfterTeamAccessChanges_CannotManageOrMoveProject(
        string action, string change, HttpStatusCode expectedStatus)
    {
        var context = await CreateContextAsync();
        SetToken(context.Member.Token);
        var destination = await new TeamTestHelper(_client).CreateTeamAsync();
        await ChangeAccessAsync(context, change);

        SetToken(context.Member.Token);
        var response = action switch
        {
            "delete" => await _client.DeleteAsync($"/api/project/{context.Project.Id}"),
            "move" => await _client.PutAsJsonAsync($"/api/project/{context.Project.Id}",
                new { TeamId = destination.Id }),
            _ => await _client.PutAsJsonAsync($"/api/project/{context.Project.Id}",
                new { Name = "Unauthorized change" })
        };

        Assert.Equal(expectedStatus, response.StatusCode);
        using var scope = _factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
        var project = await database.Projects.Find(p => p.Id == context.Project.Id).SingleAsync();
        Assert.Equal(context.Project.Name, project.Name);
        Assert.Equal(context.Team.Id, project.TeamId);
        Assert.Null(project.DeletedAt);
    }

    [Theory]
    [InlineData("removed")]
    [InlineData("left")]
    public async Task FormerMember_CannotReadOrModifyTeamResources(string change)
    {
        var context = await CreateContextAsync();
        await ChangeAccessAsync(context, change);
        SetToken(context.Member.Token);

        var responses = new[]
        {
            await _client.GetAsync($"/api/team/{context.Team.Id}"),
            await _client.GetAsync($"/api/team/{context.Team.Id}/members"),
            await _client.GetAsync($"/api/project/{context.Project.Id}"),
            await _client.GetAsync($"/api/project/team/{context.Team.Id}"),
            await _client.GetAsync($"/api/todotask/{context.Task.Id}"),
            await _client.GetAsync($"/api/todotask/project/{context.Project.Id}"),
            await _client.PostAsJsonAsync("/api/project", new
                { Name = "Blocked", Description = "Blocked", TeamId = context.Team.Id }),
            await _client.PostAsJsonAsync("/api/todotask", new
                { Title = "Blocked", Description = "Blocked", ProjectId = context.Project.Id }),
            await _client.PutAsJsonAsync($"/api/todotask/{context.Task.Id}", new
                { Title = "Blocked", Description = "Blocked" }),
            await _client.PostAsJsonAsync($"/api/todotask/{context.Task.Id}/assign", new
                { UserId = context.Owner.User.Id }),
            await _client.DeleteAsync($"/api/todotask/{context.Task.Id}")
        };

        Assert.All(responses, response => Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode));
        using var scope = _factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
        var task = await database.TodoTasks.Find(t => t.Id == context.Task.Id).SingleAsync();
        Assert.Equal(context.Task.Title, task.Title);
        Assert.Null(task.AssignedUserId);
        Assert.Null(task.DeletedAt);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CurrentProjectOrTeamOwner_CanUpdateAndDeleteProject(bool asTeamOwner)
    {
        var context = await CreateContextAsync();
        SetToken(asTeamOwner ? context.Owner.Token : context.Member.Token);

        var update = await _client.PutAsJsonAsync($"/api/project/{context.Project.Id}",
            new { Name = "Authorized change" });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        var updated = await update.Content.ReadFromJsonAsync<ProjectResponseDto>();
        Assert.Equal("Authorized change", updated!.Name);

        var delete = await _client.DeleteAsync($"/api/project/{context.Project.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
    }

    [Fact]
    public async Task ReaddedMember_CanStartReassignedTaskWithOriginalToken()
    {
        var context = await CreateContextAsync();
        await ChangeAccessAsync(context, "removed");
        SetToken(context.Member.Token);
        var blocked = await _client.PostAsync($"/api/todotask/{context.Task.Id}/start", null);
        Assert.Equal(HttpStatusCode.Forbidden, blocked.StatusCode);

        SetToken(context.Owner.Token);
        (await _client.PostAsync(
            $"/api/team/{context.Team.Id}/add-member?userId={context.Member.User.Id}", null))
            .EnsureSuccessStatusCode();
        (await _client.PostAsJsonAsync($"/api/todotask/{context.Task.Id}/assign",
            new { UserId = context.Member.User.Id })).EnsureSuccessStatusCode();
        SetToken(context.Member.Token);
        var allowed = await _client.PostAsync($"/api/todotask/{context.Task.Id}/start", null);
        Assert.Equal(HttpStatusCode.OK, allowed.StatusCode);
    }

    private async Task<PermissionContext> CreateContextAsync()
    {
        var users = new UserTestHelper(_client);
        var owner = await users.CreateUserAndLoginAsync();
        var member = await users.CreateUserAndLoginAsync();
        SetToken(owner.Token);
        var team = await new TeamTestHelper(_client).CreateTeamAsync();
        (await _client.PostAsync($"/api/team/{team.Id}/add-member?userId={member.User.Id}", null))
            .EnsureSuccessStatusCode();

        SetToken(member.Token);
        var project = await new ProjectTestHelper(_client).CreateProjectAsync(team.Id);
        var response = await _client.PostAsJsonAsync("/api/todotask", new
        {
            Title = "Assigned before access change", Description = "Security regression",
            ProjectId = project.Id, AssignedUserId = member.User.Id
        });
        response.EnsureSuccessStatusCode();
        var task = await response.Content.ReadFromJsonAsync<TodoTaskResponseDto>();
        return new PermissionContext(owner, member, team, project, task!);
    }

    private async Task ChangeAccessAsync(PermissionContext context, string change)
    {
        SetToken(change == "left" ? context.Member.Token : context.Owner.Token);
        var response = change switch
        {
            "removed" => await _client.DeleteAsync(
                $"/api/team/{context.Team.Id}/remove-member?userId={context.Member.User.Id}"),
            "left" => await _client.DeleteAsync($"/api/team/{context.Team.Id}/leave"),
            "team-inactive" => await _client.PutAsJsonAsync($"/api/team/{context.Team.Id}",
                new { IsActive = false }),
            "team-deleted" => await _client.DeleteAsync($"/api/team/{context.Team.Id}"),
            "project-inactive" => await _client.PutAsJsonAsync($"/api/project/{context.Project.Id}",
                new { Status = ProjectStatus.Inactive }),
            "project-deleted" => await _client.DeleteAsync($"/api/project/{context.Project.Id}"),
            "reassigned" => await _client.PostAsJsonAsync($"/api/todotask/{context.Task.Id}/assign",
                new { UserId = context.Owner.User.Id }),
            _ => throw new ArgumentOutOfRangeException(nameof(change))
        };
        response.EnsureSuccessStatusCode();
    }

    private void SetToken(string token) =>
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    private static TheoryData<string, string, HttpStatusCode> BuildCases(
        string[] actions, (string Change, HttpStatusCode Status)[] changes)
    {
        var cases = new TheoryData<string, string, HttpStatusCode>();
        foreach (var action in actions)
        foreach (var change in changes)
            cases.Add(action, change.Change, change.Status);
        return cases;
    }

    private sealed record PermissionContext(
        LoginResponseDto Owner, LoginResponseDto Member, TeamResponseDto Team,
        ProjectResponseDto Project, TodoTaskResponseDto Task);
}
