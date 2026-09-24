using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Rivulus.Application.DTOs;

namespace Rivulus.IntegrationTests;

public class ProjectTransferIntegrationTests(RivulusApiFactory factory) : IClassFixture<RivulusApiFactory>
{
    [Fact]
    public async Task MoveProject_RejectsIncompatibleOwnerOrAssignee_WithoutChangingProject()
    {
        using var client = factory.CreateClient();
        var users = new UserTestHelper(client);
        var owner = await users.CreateUserAndLoginAsync();
        var projectOwner = await users.CreateUserAndLoginAsync();
        var destinationMember = await users.CreateUserAndLoginAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", owner.Token);
        var teams = new TeamTestHelper(client);
        var source = await teams.CreateTeamAsync();
        var destination = await teams.CreateTeamAsync();
        (await client.PostAsync($"/api/team/{source.Id}/add-member?userId={projectOwner.User.Id}", null)).EnsureSuccessStatusCode();
        (await client.PostAsync($"/api/team/{destination.Id}/add-member?userId={destinationMember.User.Id}", null)).EnsureSuccessStatusCode();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", projectOwner.Token);
        var project = await new ProjectTestHelper(client).CreateProjectAsync(source.Id);
        (await client.PostAsJsonAsync("/api/todotask", new { Title = "Assigned", Description = "Task", ProjectId = project.Id, AssignedUserId = projectOwner.User.Id })).EnsureSuccessStatusCode();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", owner.Token);

        var ownerBlocked = await client.PutAsJsonAsync($"/api/project/{project.Id}", new { TeamId = destination.Id });
        Assert.Equal(HttpStatusCode.Conflict, ownerBlocked.StatusCode);
        var assigneeBlocked = await client.PutAsJsonAsync($"/api/project/{project.Id}", new { TeamId = destination.Id, OwnerId = destinationMember.User.Id });
        Assert.Equal(HttpStatusCode.Conflict, assigneeBlocked.StatusCode);
        var current = await client.GetFromJsonAsync<ProjectResponseDto>($"/api/project/{project.Id}");
        Assert.Equal(source.Id, current!.TeamId);
        Assert.Equal(projectOwner.User.Id, current.OwnerId);
    }

    [Fact]
    public async Task MoveProject_AllCompatibleMembers_TransfersProjectAndOwner()
    {
        using var client = factory.CreateClient();
        var users = new UserTestHelper(client);
        var owner = await users.CreateUserAndLoginAsync();
        var destinationMember = await users.CreateUserAndLoginAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", owner.Token);
        var teams = new TeamTestHelper(client);
        var source = await teams.CreateTeamAsync();
        var destination = await teams.CreateTeamAsync();
        (await client.PostAsync($"/api/team/{destination.Id}/add-member?userId={destinationMember.User.Id}", null)).EnsureSuccessStatusCode();
        var project = await new ProjectTestHelper(client).CreateProjectAsync(source.Id);
        (await client.PostAsJsonAsync("/api/todotask", new { Title = "Assigned", Description = "Task", ProjectId = project.Id, AssignedUserId = owner.User.Id })).EnsureSuccessStatusCode();

        var move = await client.PutAsJsonAsync($"/api/project/{project.Id}", new { TeamId = destination.Id, OwnerId = destinationMember.User.Id });
        move.EnsureSuccessStatusCode();
        var updated = await move.Content.ReadFromJsonAsync<ProjectResponseDto>();
        Assert.Equal(destination.Id, updated!.TeamId);
        Assert.Equal(destinationMember.User.Id, updated.OwnerId);
    }
}
