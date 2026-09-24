using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Rivulus.Application.DTOs;

namespace Rivulus.IntegrationTests;

public class NotificationIntegrationTests(RivulusApiFactory factory) : IClassFixture<RivulusApiFactory>
{
    [Fact]
    public async Task TaskAssignment_CreatesPrivateNotification_ThatRecipientCanRead()
    {
        using var client = factory.CreateClient();
        var users = new UserTestHelper(client);
        var owner = await users.CreateUserAndLoginAsync();
        var member = await users.CreateUserAndLoginAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", owner.Token);
        var team = await new TeamTestHelper(client).CreateTeamAsync();
        (await client.PostAsync($"/api/team/{team.Id}/add-member?userId={member.User.Id}", null)).EnsureSuccessStatusCode();
        var project = await new ProjectTestHelper(client).CreateProjectAsync(team.Id);
        (await client.PostAsJsonAsync("/api/todotask", new { Title = "Assigned", Description = "Notification", ProjectId = project.Id, AssignedUserId = member.User.Id })).EnsureSuccessStatusCode();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", member.Token);
        var notifications = await client.GetFromJsonAsync<List<UserNotificationResponseDto>>("/api/notification");
        var notification = Assert.Single(notifications!);
        Assert.Contains("Assigned", notification.Message);
        Assert.Null(notification.ReadAt);
        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsync($"/api/notification/{notification.Id}/read", null)).StatusCode);
        Assert.NotNull(Assert.Single((await client.GetFromJsonAsync<List<UserNotificationResponseDto>>("/api/notification"))!).ReadAt);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", owner.Token);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsync($"/api/notification/{notification.Id}/read", null)).StatusCode);
    }
}
