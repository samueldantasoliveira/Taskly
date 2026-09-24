using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Rivulus.Application.DTOs;
using Rivulus.Domain;
using Rivulus.Domain.Entities;
using Rivulus.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Rivulus.IntegrationTests;

public class TodoTaskIntegrationTests : IClassFixture<RivulusApiFactory>
{
    private readonly HttpClient _client;
    private readonly RivulusApiFactory _factory;
    private readonly UserTestHelper _userHelper;
    private readonly TeamTestHelper _teamHelper;
    private readonly ProjectTestHelper _projectHelper;

    public TodoTaskIntegrationTests(RivulusApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _userHelper = new UserTestHelper(_client);
        _teamHelper = new TeamTestHelper(_client);
        _projectHelper = new ProjectTestHelper(_client);
    }

    [Fact]
    public async Task Comments_AuthorCanEditAndDelete_ButAnotherMemberCannot()
    {
        var owner = await _userHelper.CreateUserAndLoginAsync();
        var member = await _userHelper.CreateUserAndLoginAsync();
        SetBearerToken(owner.Token);
        var team = await _teamHelper.CreateTeamAsync();
        await AddMemberAsync(team.Id, member.User.Id);
        var project = await _projectHelper.CreateProjectAsync(team.Id);
        var task = await CreateTaskAsync(project.Id, owner.User.Id);
        var createdResponse = await _client.PostAsJsonAsync($"/api/todotask/{task.Id}/comments", new { Content = "Primeiro comentário" });
        createdResponse.EnsureSuccessStatusCode();
        var created = (await createdResponse.Content.ReadFromJsonAsync<TaskCommentResponseDto>())!;

        SetBearerToken(member.Token);
        Assert.Equal(HttpStatusCode.Forbidden, (await _client.PutAsJsonAsync($"/api/todotask/{task.Id}/comments/{created.Id}", new { Content = "Alteração indevida" })).StatusCode);

        SetBearerToken(owner.Token);
        var update = await _client.PutAsJsonAsync($"/api/todotask/{task.Id}/comments/{created.Id}", new { Content = "Comentário editado" });
        update.EnsureSuccessStatusCode();
        Assert.Equal("Comentário editado", (await update.Content.ReadFromJsonAsync<TaskCommentResponseDto>())!.Content);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/todotask/{task.Id}/comments/{created.Id}")).StatusCode);
        Assert.Empty((await _client.GetFromJsonAsync<List<TaskCommentResponseDto>>($"/api/todotask/{task.Id}/comments"))!);
    }

    [Fact]
    public async Task ProjectActivity_RecordsTaskChanges_AndRestrictsProjectAccess()
    {
        var owner = await _userHelper.CreateUserAndLoginAsync();
        SetBearerToken(owner.Token);
        var team = await _teamHelper.CreateTeamAsync();
        var project = await _projectHelper.CreateProjectAsync(team.Id);
        var task = await CreateTaskAsync(project.Id, owner.User.Id);

        var activities = await _client.GetFromJsonAsync<List<ProjectActivityResponseDto>>($"/api/todotask/project/{project.Id}/activity");

        var activity = Assert.Single(activities!);
        Assert.Equal(owner.User.Id, activity.ActorId);
        Assert.Equal(task.Id, activity.TaskId);
        Assert.Equal("criou a tarefa", activity.Description);

        var outsider = await _userHelper.CreateUserAndLoginAsync();
        SetBearerToken(outsider.Token);
        Assert.Equal(HttpStatusCode.Forbidden, (await _client.GetAsync($"/api/todotask/project/{project.Id}/activity")).StatusCode);
    }

    [Fact]
    public async Task MyWork_ReturnsOnlyAuthenticatedUsersActiveTasks_WithProjectContext()
    {
        var owner = await _userHelper.CreateUserAndLoginAsync();
        var member = await _userHelper.CreateUserAndLoginAsync();
        SetBearerToken(owner.Token);
        var team = await _teamHelper.CreateTeamAsync();
        await AddMemberAsync(team.Id, member.User.Id);
        var project = await _projectHelper.CreateProjectAsync(team.Id);
        var mine = await CreateTaskAsync(project.Id, owner.User.Id);
        await CreateTaskAsync(project.Id, member.User.Id);

        var response = await _client.GetAsync("/api/todotask/my-work");

        response.EnsureSuccessStatusCode();
        var dashboard = await response.Content.ReadFromJsonAsync<MyWorkDashboardResponseDto>();
        Assert.NotNull(dashboard);
        Assert.Equal(1, dashboard.TodoCount);
        var item = Assert.Single(dashboard.Items);
        Assert.Equal(mine.Id, item.Id);
        Assert.Equal(project.Id, item.ProjectId);
        Assert.Equal(project.Name, item.ProjectName);
        Assert.Equal(team.Name, item.TeamName);
    }

    [Fact]
    public async Task TodoTask_PersistsPriorityAndDueDate_AndSupportsDueDateSorting()
    {
        var owner = await _userHelper.CreateUserAndLoginAsync();
        SetBearerToken(owner.Token);
        var team = await _teamHelper.CreateTeamAsync();
        var project = await _projectHelper.CreateProjectAsync(team.Id);
        var response = await _client.PostAsJsonAsync("/api/todotask", new
        {
            Title = "Important", Description = "Has a date", ProjectId = project.Id,
            AssignedUserId = owner.User.Id, Priority = TaskPriority.High, DueDate = "2026-04-10"
        });
        response.EnsureSuccessStatusCode();
        var task = (await response.Content.ReadFromJsonAsync<TodoTaskResponseDto>())!;
        Assert.Equal(TaskPriority.High, task.Priority);
        Assert.Equal(new DateTime(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc), task.DueDate);

        var page = await GetPageAsync(project.Id, "?sortBy=DueDate&sortDirection=Ascending");
        Assert.Equal(task.Id, Assert.Single(page.Items).Id);
        var invalid = await _client.PostAsJsonAsync("/api/todotask", new { Title = "Invalid", Description = "", ProjectId = project.Id, Priority = 99 });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
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

    [Fact]
    public async Task GetByProjectId_FiltersCanBeCombinedAndTotalCountUsesSameFilter()
    {
        var owner = await _userHelper.CreateUserAndLoginAsync();
        var member = await _userHelper.CreateUserAndLoginAsync();
        SetBearerToken(owner.Token);
        var team = await _teamHelper.CreateTeamAsync();
        await AddMemberAsync(team.Id, member.User.Id);
        var project = await _projectHelper.CreateProjectAsync(team.Id);
        var date = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);

        var todo = new TodoTask("Alpha API", "Filters", project.Id, owner.User.Id)
        {
            CreatedAt = date.AddDays(1)
        };
        var inProgress = new TodoTask("Bravo API", "Filters", project.Id, member.User.Id)
        {
            CreatedAt = date.AddDays(3)
        };
        inProgress.Start();
        var done = new TodoTask("Charlie Web", "Filters", project.Id, member.User.Id)
        {
            CreatedAt = date.AddDays(2)
        };
        done.Start();
        done.Complete();
        var cancelled = new TodoTask("Delta Archive", "Filters", project.Id, owner.User.Id)
        {
            CreatedAt = date.AddDays(4)
        };
        cancelled.Cancel();
        var deleted = new TodoTask("Deleted API", "Excluded", project.Id, member.User.Id);
        deleted.Delete();

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
        await context.TodoTasks.InsertManyAsync([todo, inProgress, done, cancelled, deleted]);

        var byTitle = await GetPageAsync(project.Id, "?title=api&pageSize=100");
        Assert.Equal(2, byTitle.TotalCount);
        Assert.Equal(
            [inProgress.Id, todo.Id],
            byTitle.Items.Select(task => task.Id));

        var byStatus = await GetPageAsync(project.Id, "?status=Done&pageSize=100");
        Assert.Equal(1, byStatus.TotalCount);
        Assert.Equal(done.Id, Assert.Single(byStatus.Items).Id);

        var byAssignee = await GetPageAsync(
            project.Id,
            $"?assigneeId={member.User.Id}&pageSize=100");
        Assert.Equal(2, byAssignee.TotalCount);
        Assert.Equal(
            [inProgress.Id, done.Id],
            byAssignee.Items.Select(task => task.Id));

        var combined = await GetPageAsync(
            project.Id,
            $"?title=api&status=InProgress&assigneeId={member.User.Id}&pageSize=100");
        Assert.Equal(1, combined.TotalCount);
        Assert.Equal(inProgress.Id, Assert.Single(combined.Items).Id);
    }

    [Fact]
    public async Task GetByProjectId_SortsBySupportedFieldsInBothDirections()
    {
        var owner = await _userHelper.CreateUserAndLoginAsync();
        SetBearerToken(owner.Token);
        var team = await _teamHelper.CreateTeamAsync();
        var project = await _projectHelper.CreateProjectAsync(team.Id);
        var date = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);

        var alpha = new TodoTask("Alpha", "Sorting", project.Id, owner.User.Id)
        {
            CreatedAt = date.AddDays(1)
        };
        var bravo = new TodoTask("Bravo", "Sorting", project.Id, owner.User.Id)
        {
            CreatedAt = date.AddDays(3)
        };
        bravo.Start();
        var charlie = new TodoTask("Charlie", "Sorting", project.Id, owner.User.Id)
        {
            CreatedAt = date.AddDays(2)
        };
        charlie.Start();
        charlie.Complete();
        var delta = new TodoTask("Delta", "Sorting", project.Id, owner.User.Id)
        {
            CreatedAt = date.AddDays(4)
        };
        delta.Cancel();

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
        await context.TodoTasks.InsertManyAsync([alpha, bravo, charlie, delta]);

        var titleAscending = await GetPageAsync(
            project.Id,
            "?sortBy=Title&sortDirection=Ascending&pageSize=100");
        Assert.Equal(
            ["Alpha", "Bravo", "Charlie", "Delta"],
            titleAscending.Items.Select(task => task.Title));

        var titleDescending = await GetPageAsync(
            project.Id,
            "?sortBy=Title&sortDirection=Descending&pageSize=100");
        Assert.Equal(
            ["Delta", "Charlie", "Bravo", "Alpha"],
            titleDescending.Items.Select(task => task.Title));

        var statusAscending = await GetPageAsync(
            project.Id,
            "?sortBy=Status&sortDirection=Ascending&pageSize=100");
        Assert.Equal(
            [TodoStatus.Todo, TodoStatus.InProgress, TodoStatus.Done, TodoStatus.Cancelled],
            statusAscending.Items.Select(task => task.Status));

        var statusDescending = await GetPageAsync(
            project.Id,
            "?sortBy=Status&sortDirection=Descending&pageSize=100");
        Assert.Equal(
            [TodoStatus.Cancelled, TodoStatus.Done, TodoStatus.InProgress, TodoStatus.Todo],
            statusDescending.Items.Select(task => task.Status));

        var createdAscending = await GetPageAsync(
            project.Id,
            "?sortBy=CreatedAt&sortDirection=Ascending&pageSize=100");
        Assert.Equal(
            [alpha.Id, charlie.Id, bravo.Id, delta.Id],
            createdAscending.Items.Select(task => task.Id));

        var createdDescending = await GetPageAsync(
            project.Id,
            "?sortBy=CreatedAt&sortDirection=Descending&pageSize=100");
        Assert.Equal(
            [delta.Id, bravo.Id, charlie.Id, alpha.Id],
            createdDescending.Items.Select(task => task.Id));
    }

    [Theory]
    [InlineData("status=999", "Status")]
    [InlineData("sortBy=999", "SortBy")]
    [InlineData("sortDirection=999", "SortDirection")]
    public async Task GetByProjectId_InvalidEnum_ReturnsValidationProblem(
        string query,
        string field)
    {
        var owner = await _userHelper.CreateUserAndLoginAsync();
        SetBearerToken(owner.Token);
        var team = await _teamHelper.CreateTeamAsync();
        var project = await _projectHelper.CreateProjectAsync(team.Id);

        var response = await _client.GetAsync(
            $"/api/TodoTask/project/{project.Id}?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content
            .ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
        Assert.Contains(
            problem.Errors.Keys,
            key => key.Contains(field, StringComparison.OrdinalIgnoreCase));
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
