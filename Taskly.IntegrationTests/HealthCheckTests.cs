using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Taskly.Infrastructure;

namespace Taskly.IntegrationTests;

public class HealthCheckTests : IClassFixture<TasklyApiFactory>
{
    private readonly HttpClient _client;
    private readonly TasklyApiFactory _factory;

    public HealthCheckTests(TasklyApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Api_ShouldStart_AndReturnOk()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var mongoDbContext = scope.ServiceProvider
            .GetRequiredService<MongoDbContext>();

        await mongoDbContext.EnsureIndexesAsync();

        var response = await _client.GetAsync("/health");

        response.EnsureSuccessStatusCode();
    }
}
