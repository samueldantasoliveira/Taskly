using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Taskly.Application.DTOs;

namespace Taskly.IntegrationTests;

public class ProjectIntegrationTests : IClassFixture<TasklyApiFactory>
{
    private readonly HttpClient _client;
    private readonly UserTestHelper _userHelper;
    private readonly TeamTestHelper _teamHelper;
    private readonly ProjectTestHelper _projectHelper;

    public ProjectIntegrationTests(TasklyApiFactory factory)
    {
        _client = factory.CreateClient();
        _userHelper = new UserTestHelper(_client);
        _teamHelper = new TeamTestHelper(_client);
        _projectHelper = new ProjectTestHelper(_client);
    }

    [Fact]
    public async Task GetTeamProjects_ReturnsOnlyNonDeletedProjectsFromRequestedTeam()
    {
        var login = await _userHelper.CreateUserAndLoginAsync();
        SetBearerToken(login.Token);

        var requestedTeam = await _teamHelper.CreateTeamAsync();
        var otherTeam = await _teamHelper.CreateTeamAsync();

        var visibleProject = await _projectHelper.CreateProjectAsync(
            requestedTeam.Id,
            "Visible project");
        var deletedProject = await _projectHelper.CreateProjectAsync(
            requestedTeam.Id,
            "Deleted project");
        await _projectHelper.CreateProjectAsync(otherTeam.Id, "Other team project");

        var deleteResponse = await _client.DeleteAsync(
            $"/api/project/{deletedProject.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var response = await _client.GetAsync(
            $"/api/project/team/{requestedTeam.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var projects = await response.Content
            .ReadFromJsonAsync<List<ProjectResponseDto>>();

        Assert.NotNull(projects);
        var project = Assert.Single(projects);
        Assert.Equal(visibleProject.Id, project.Id);
        Assert.Equal(visibleProject.Name, project.Name);
        Assert.Equal(visibleProject.Description, project.Description);
        Assert.Equal(visibleProject.OwnerId, project.OwnerId);
        Assert.Equal(visibleProject.Status, project.Status);
        Assert.Equal(requestedTeam.Id, project.TeamId);
    }

    [Fact]
    public async Task GetTeamProjects_NonMemberReturnsForbidden()
    {
        var teamOwnerLogin = await _userHelper.CreateUserAndLoginAsync();
        SetBearerToken(teamOwnerLogin.Token);
        var team = await _teamHelper.CreateTeamAsync();

        var outsiderLogin = await _userHelper.CreateUserAndLoginAsync();
        SetBearerToken(outsiderLogin.Token);

        var response = await _client.GetAsync($"/api/project/team/{team.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private void SetBearerToken(string token)
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }
}
