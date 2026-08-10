using Microsoft.AspNetCore.Mvc.Testing;

namespace Taskly.IntegrationTests;

public class HealthCheckTests : IClassFixture<TasklyApiFactory>
{
    private readonly HttpClient _client;

    public HealthCheckTests(TasklyApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Api_ShouldStart_AndReturnOk()
    {
        var response = await _client.GetAsync("/health");

        response.EnsureSuccessStatusCode();
    }
}