using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Taskly.Application.DTOs;
using Taskly.Domain;

namespace Taskly.IntegrationTests;

public class TodoTaskIntegrationTests : IClassFixture<TasklyApiFactory>
{
    private readonly HttpClient _client;
    private readonly UserTestHelper _userHelper;
    private readonly TeamTestHelper _teamHelper;
    private readonly ProjectTestHelper _projectHelper;

    public TodoTaskIntegrationTests(TasklyApiFactory factory)
    {
        _client = factory.CreateClient();
        _userHelper = new UserTestHelper(_client);
        _teamHelper = new TeamTestHelper(_client);
        _projectHelper = new ProjectTestHelper(_client);
    }

    [Fact]
    public async Task TodoTaskLifecycle_AssignedUserCanStartAndCompleteTask()
    {
        var owner = await _userHelper.CreateUserAndLoginAsync();
        var member = await _userHelper.CreateUserAndLoginAsync();

        SetBearerToken(owner.Token);
        var team = await _teamHelper.CreateTeamAsync();
        await AddMemberAsync(team.Id, member.User.Id);
        var project = await _projectHelper.CreateProjectAsync(team.Id);
        var task = await CreateTaskAsync(project.Id, member.User.Id);

        SetBearerToken(member.Token);
        var startResponse = await _client.PostAsync(
            $"/api/TodoTask/{task.Id}/start",
            content: null);
        Assert.Equal(HttpStatusCode.OK, startResponse.StatusCode);

        var completeResponse = await _client.PostAsync(
            $"/api/TodoTask/{task.Id}/complete",
            content: null);
        Assert.Equal(HttpStatusCode.OK, completeResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/TodoTask/{task.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var completedTask = await getResponse.Content
            .ReadFromJsonAsync<TodoTaskResponseDto>();
        Assert.NotNull(completedTask);
        Assert.Equal(TodoStatus.Done, completedTask.Status);
        Assert.Equal(member.User.Id, completedTask.AssignedUserId);

        var listResponse = await _client.GetAsync(
            $"/api/TodoTask/project/{project.Id}");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var projectTasks = await listResponse.Content
            .ReadFromJsonAsync<List<TodoTaskResponseDto>>();
        Assert.NotNull(projectTasks);
        var listedTask = Assert.Single(projectTasks);
        Assert.Equal(task.Id, listedTask.Id);
        Assert.Equal(TodoStatus.Done, listedTask.Status);
    }

    [Fact]
    public async Task TodoTask_UserOutsideTeamCannotReadOrModifyTask()
    {
        var owner = await _userHelper.CreateUserAndLoginAsync();
        SetBearerToken(owner.Token);

        var team = await _teamHelper.CreateTeamAsync();
        var project = await _projectHelper.CreateProjectAsync(team.Id);
        var task = await CreateTaskAsync(project.Id, owner.User.Id);

        var outsider = await _userHelper.CreateUserAndLoginAsync();
        SetBearerToken(outsider.Token);

        var readResponse = await _client.GetAsync($"/api/TodoTask/{task.Id}");
        var listResponse = await _client.GetAsync(
            $"/api/TodoTask/project/{project.Id}");
        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/TodoTask/{task.Id}",
            new
            {
                Title = "Unauthorized update",
                Description = "An outsider must not be allowed to change this task"
            });

        Assert.True(
            readResponse.StatusCode == HttpStatusCode.Forbidden &&
            listResponse.StatusCode == HttpStatusCode.Forbidden &&
            updateResponse.StatusCode == HttpStatusCode.Forbidden,
            $"Expected read, list and update to return Forbidden, but read returned " +
            $"{readResponse.StatusCode}, list returned {listResponse.StatusCode} and " +
            $"update returned {updateResponse.StatusCode}.");
    }

    private async Task AddMemberAsync(Guid teamId, Guid userId)
    {
        var response = await _client.PostAsync(
            $"/api/team/{teamId}/add-member?userId={userId}",
            content: null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task<TodoTaskResponseDto> CreateTaskAsync(
        Guid projectId,
        Guid assignedUserId)
    {
        var response = await _client.PostAsJsonAsync("/api/TodoTask", new
        {
            Title = $"Integration task {Guid.NewGuid()}",
            Description = "Task created for an integration flow",
            ProjectId = projectId,
            AssignedUserId = assignedUserId
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var task = await response.Content.ReadFromJsonAsync<TodoTaskResponseDto>();
        Assert.NotNull(task);
        return task;
    }

    private void SetBearerToken(string token)
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }
}
