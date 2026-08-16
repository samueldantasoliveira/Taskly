using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using Taskly.Domain.Entities;

namespace Taskly.IntegrationTests;

public class TodoTaskIntegrationTests : IClassFixture<TasklyApiFactory>
{
    private readonly HttpClient _client;
    private readonly UserTestHelper _userHelper;

    public TodoTaskIntegrationTests(TasklyApiFactory factory)
    {
        _client = factory.CreateClient();
        _userHelper = new UserTestHelper(_client);
    }

    [Fact]
    public async Task CreateTodoTask_ValidData_ReturnsOk()
    {
        var loginResult = await _userHelper.CreateUserAndLoginAsync();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginResult.Token);
        
        var TodoTask = new
        {
            Title = "Title Test",
            Description = "Description Test"
        };
    }
}