using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Taskly.Application.DTOs;
using Taskly.Domain;
using Taskly.Domain.Entities;
using Taskly.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Taskly.IntegrationTests;

public class TodoTaskIntegrationTests : IClassFixture<TasklyApiFactory>
{
    private readonly HttpClient _client;
    private readonly TasklyApiFactory _factory;
    private readonly UserTestHelper _userHelper;
    private readonly TeamTestHelper _teamHelper;
    private readonly ProjectTestHelper _projectHelper;

    public TodoTaskIntegrationTests(TasklyApiFactory factory)
    {
        _factory = factory;
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
            .ReadFromJsonAsync<PagedResult<TodoTaskResponseDto>>();
        Assert.NotNull(projectTasks);
        Assert.Equal(1, projectTasks.TotalCount);
        var listedTask = Assert.Single(projectTasks.Items);
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

    [Theory]
    [InlineData("page=0", "Page must be greater than or equal to 1.")]
    [InlineData("page=-1", "Page must be greater than or equal to 1.")]
    [InlineData("pageSize=0", "Page size must be between 1 and 100.")]
    [InlineData("pageSize=-1", "Page size must be between 1 and 100.")]
    [InlineData("pageSize=101", "Page size must be between 1 and 100.")]
    [InlineData("page=2147483647&pageSize=100", "The requested page exceeds the supported pagination limit.")]
    public async Task GetByProjectId_InvalidPagination_ReturnsBadRequest(string query, string message)
    {
        var user = await _userHelper.CreateUserAndLoginAsync();
        SetBearerToken(user.Token);

        var response = await _client.GetAsync($"/api/TodoTask/project/{Guid.NewGuid()}?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(message, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task GetByProjectId_Pagination_OrdersAndCountsOnlyActiveProjectTasks()
    {
        var owner = await _userHelper.CreateUserAndLoginAsync();
        SetBearerToken(owner.Token);
        var team = await _teamHelper.CreateTeamAsync();
        var project = await _projectHelper.CreateProjectAsync(team.Id);
        var otherProject = await _projectHelper.CreateProjectAsync(team.Id);
        var date = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var tasks = Enumerable.Range(0, 25)
            .Select(index => new TodoTask($"Task {index}", "Pagination fixture", project.Id, owner.User.Id)
            {
                // Repeated timestamps exercise the Id tie-breaker across page boundaries.
                CreatedAt = date.AddDays(index % 3)
            }).ToList();
        var deleted = new TodoTask("Deleted", "Excluded", project.Id, owner.User.Id)
        {
            CreatedAt = date.AddDays(10)
        };
        deleted.Delete();
        var unrelated = new TodoTask("Other project", "Excluded", otherProject.Id, owner.User.Id);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
        await context.TodoTasks.InsertManyAsync(tasks.Concat([deleted, unrelated]));

        // Ids are persisted as strings, so use their ordinal string order.
        var expected = tasks.OrderByDescending(task => task.CreatedAt)
            .ThenBy(task => task.Id.ToString(), StringComparer.Ordinal)
            .Select(task => task.Id).ToArray();

        var defaults = await GetPageAsync(project.Id);
        Assert.Equal(25, defaults.TotalCount);
        Assert.Equal(expected.Take(20), defaults.Items.Select(task => task.Id));

        var secondDefaultPage = await GetPageAsync(project.Id, "?page=2");
        Assert.Equal(expected.Skip(20), secondDefaultPage.Items.Select(task => task.Id));

        var collected = new List<Guid>();
        for (var page = 1; page <= 4; page++)
        {
            var result = await GetPageAsync(project.Id, $"?page={page}&pageSize=7");
            Assert.Equal(25, result.TotalCount);
            Assert.Equal(expected.Skip((page - 1) * 7).Take(7), result.Items.Select(task => task.Id));
            collected.AddRange(result.Items.Select(task => task.Id));
        }
        Assert.Equal(expected, collected);
        Assert.Equal(25, collected.Distinct().Count());

        var beyondLastPage = await GetPageAsync(project.Id, "?page=5&pageSize=7");
        Assert.Empty(beyondLastPage.Items);
        Assert.Equal(25, beyondLastPage.TotalCount);

        var maxPageSize = await GetPageAsync(project.Id, "?pageSize=100");
        Assert.Equal(expected, maxPageSize.Items.Select(task => task.Id));

        var emptyProject = await _projectHelper.CreateProjectAsync(team.Id);
        var empty = await GetPageAsync(emptyProject.Id);
        Assert.Empty(empty.Items);
        Assert.Equal(0, empty.TotalCount);
    }

    private async Task<PagedResult<TodoTaskResponseDto>> GetPageAsync(Guid projectId, string query = "")
    {
        var response = await _client.GetAsync($"/api/TodoTask/project/{projectId}{query}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<TodoTaskResponseDto>>();
        Assert.NotNull(page);
        return page;
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
