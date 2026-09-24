using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Taskly.Application.DTOs;

namespace Taskly.IntegrationTests;

public class TeamInvitationIntegrationTests(TasklyApiFactory factory) : IClassFixture<TasklyApiFactory>
{
    [Fact]
    public async Task Invitation_CanOnlyBeAcceptedByMatchingEmail_AndAddsMember()
    {
        using var client = factory.CreateClient();
        var users = new UserTestHelper(client);
        var owner = await users.CreateUserAndLoginAsync();
        var invited = await users.CreateUserAndLoginAsync();
        var outsider = await users.CreateUserAndLoginAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", owner.Token);
        var team = await new TeamTestHelper(client).CreateTeamAsync();
        var create = await client.PostAsJsonAsync($"/api/team/{team.Id}/invitations", new { invited.User.Email });
        create.EnsureSuccessStatusCode();
        var invitation = await create.Content.ReadFromJsonAsync<TeamInvitationResponseDto>();
        Assert.False(string.IsNullOrWhiteSpace(invitation!.Token));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", outsider.Token);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsync($"/api/team/invitations/{invitation.Token}/accept", null)).StatusCode);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", invited.Token);
        var accept = await client.PostAsync($"/api/team/invitations/{invitation.Token}/accept", null);
        accept.EnsureSuccessStatusCode();
        var acceptedTeam = await accept.Content.ReadFromJsonAsync<TeamResponseDto>();
        Assert.Contains(invited.User.Id, acceptedTeam!.UserIds);

        Assert.Equal(HttpStatusCode.Gone, (await client.PostAsync($"/api/team/invitations/{invitation.Token}/accept", null)).StatusCode);
    }

    [Fact]
    public async Task Owner_CanListAndRevokePendingInvitation()
    {
        using var client = factory.CreateClient();
        var owner = await new UserTestHelper(client).CreateUserAndLoginAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", owner.Token);
        var team = await new TeamTestHelper(client).CreateTeamAsync();
        var created = await (await client.PostAsJsonAsync($"/api/team/{team.Id}/invitations", new { Email = "future@taskly.test" })).Content.ReadFromJsonAsync<TeamInvitationResponseDto>();
        var pending = await client.GetFromJsonAsync<List<TeamInvitationResponseDto>>($"/api/team/{team.Id}/invitations");
        Assert.Equal(created!.Id, Assert.Single(pending!).Id);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/team/{team.Id}/invitations/{created.Id}")).StatusCode);
        Assert.Empty((await client.GetFromJsonAsync<List<TeamInvitationResponseDto>>($"/api/team/{team.Id}/invitations"))!);
    }
}
