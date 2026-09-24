using System.Net;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Rivulus.Infrastructure;

namespace Rivulus.IntegrationTests;

public class HealthCheckTests : IClassFixture<RivulusApiFactory>
{
    private readonly HttpClient _client;
    private readonly RivulusApiFactory _factory;

    public HealthCheckTests(RivulusApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Readiness_ShouldReturnMongoDbAsHealthy()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var mongoDbContext = scope.ServiceProvider
            .GetRequiredService<MongoDbContext>();

        await mongoDbContext.EnsureIndexesAsync();

        var response = await _client.GetAsync("/health");

        response.EnsureSuccessStatusCode();

        using var body = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync()
        );

        Assert.Equal("Healthy", body.RootElement
            .GetProperty("status")
            .GetString());

        var mongoDb = Assert.Single(body.RootElement
            .GetProperty("checks")
            .EnumerateArray());

        Assert.Equal("mongodb", mongoDb.GetProperty("name").GetString());
        Assert.Equal("Healthy", mongoDb.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Liveness_ShouldReturnHealthyWithoutDependencyChecks()
    {
        var response = await _client.GetAsync("/health/live");

        response.EnsureSuccessStatusCode();

        using var body = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync()
        );

        Assert.Equal("Healthy", body.RootElement
            .GetProperty("status")
            .GetString());
        Assert.Empty(body.RootElement
            .GetProperty("checks")
            .EnumerateArray());
    }

    [Fact]
    public async Task Swagger_ShouldNotBeAvailableOutsideDevelopment()
    {
        var response = await _client.GetAsync("/swagger/index.html");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
