using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Taskly.Application;
using Taskly.Application.DTOs;
using Taskly.Domain;
using Taskly.Domain.Entities;
using Taskly.Infrastructure;

namespace Taskly.IntegrationTests;

public class OwnershipIntegrationTests(TasklyApiFactory factory) : IClassFixture<TasklyApiFactory>
{
    [Fact]
    public async Task OwnerCanTransferTeam_ThenDeleteAccount_WithoutAbandoningTeam()
    {
        using var client = factory.CreateClient();
        var helper = new UserTestHelper(client);
        var owner = await helper.CreateUserAndLoginAsync();
        var member = await helper.CreateUserAndLoginAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", owner.Token);
        var team = await new TeamTestHelper(client).CreateTeamAsync();
        Assert.Equal(HttpStatusCode.Conflict, (await client.DeleteAsync($"/api/user/{owner.User.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PutAsJsonAsync($"/api/team/{team.Id}", new { OwnerId = member.User.Id })).StatusCode);
        (await client.PostAsync($"/api/team/{team.Id}/add-member?userId={member.User.Id}", null)).EnsureSuccessStatusCode();
        var transfer = await client.PutAsJsonAsync($"/api/team/{team.Id}", new { OwnerId = member.User.Id });
        transfer.EnsureSuccessStatusCode();
        Assert.Equal(member.User.Id, (await transfer.Content.ReadFromJsonAsync<TeamResponseDto>())!.OwnerId);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PutAsJsonAsync($"/api/team/{team.Id}", new { Name = "Blocked" })).StatusCode);
        (await client.DeleteAsync($"/api/user/{owner.User.Id}")).EnsureSuccessStatusCode();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", member.Token);
        (await client.PutAsJsonAsync($"/api/team/{team.Id}", new { Name = "New owner" })).EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ProjectTransfer_RequiresMember_AndAllowsAccountDeletion()
    {
        using var client = factory.CreateClient();
        var helper = new UserTestHelper(client);
        var owner = await helper.CreateUserAndLoginAsync();
        var member = await helper.CreateUserAndLoginAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", owner.Token);
        var team = await new TeamTestHelper(client).CreateTeamAsync();
        (await client.PostAsync($"/api/team/{team.Id}/add-member?userId={member.User.Id}", null)).EnsureSuccessStatusCode();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", member.Token);
        var created = await client.PostAsJsonAsync("/api/project", new { Name = "Owned project", Description = "Transfer", TeamId = team.Id });
        created.EnsureSuccessStatusCode();
        var project = (await created.Content.ReadFromJsonAsync<ProjectResponseDto>())!;
        Assert.Equal(HttpStatusCode.Conflict, (await client.DeleteAsync($"/api/user/{member.User.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PutAsJsonAsync($"/api/project/{project.Id}", new { OwnerId = Guid.NewGuid() })).StatusCode);
        (await client.PutAsJsonAsync($"/api/project/{project.Id}", new { OwnerId = owner.User.Id })).EnsureSuccessStatusCode();
        (await client.DeleteAsync($"/api/user/{member.User.Id}")).EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Offboarding_ReleasesOnlyActiveTasks_InThatTeam_AndAdvancesVersion()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
        var responsibilities = scope.ServiceProvider.GetRequiredService<IUserResponsibilities>();
        var member = Guid.NewGuid();
        var team = new Team("Team", Guid.NewGuid());
        var project = new Project("Project", "Description", team.Id, ProjectStatus.Active, team.OwnerId);
        await db.Teams.InsertOneAsync(team);
        await db.Projects.InsertOneAsync(project);
        var active = new TodoTask("Active", "Description", project.Id, member);
        var done = new TodoTask("Done", "Description", project.Id, member);
        done.Start(); done.Complete();
        var other = new TodoTask("Other", "Description", Guid.NewGuid(), member);
        await db.TodoTasks.InsertManyAsync([active, done, other]);
        Assert.True(await responsibilities.HasPendingAsync(member));
        await responsibilities.ReleaseActiveTasksAsync(team.Id, member);
        var stored = await db.TodoTasks.Find(t => t.Id == active.Id).SingleAsync();
        Assert.Null(stored.AssignedUserId);
        Assert.Equal(1, stored.Version);
        Assert.Equal(member, (await db.TodoTasks.Find(t => t.Id == done.Id).SingleAsync()).AssignedUserId);
        Assert.Equal(member, (await db.TodoTasks.Find(t => t.Id == other.Id).SingleAsync()).AssignedUserId);
        Assert.False(await responsibilities.HasPendingAsync(member));
    }
}
