namespace Taskly.IntegrationTests;

public class RequestObservabilityIntegrationTests(TasklyApiFactory factory) : IClassFixture<TasklyApiFactory>
{
    [Fact]
    public async Task Request_ReturnsGeneratedCorrelationId()
    {
        using var client = factory.CreateClient();
        var response = await client.GetAsync("/health/live");
        Assert.True(response.Headers.TryGetValues("X-Correlation-ID", out var values));
        Assert.True(Guid.TryParse(Assert.Single(values), out _));
    }

    [Fact]
    public async Task Request_PreservesValidCorrelationId()
    {
        using var client = factory.CreateClient();
        var expected = Guid.NewGuid().ToString("D");
        client.DefaultRequestHeaders.Add("X-Correlation-ID", expected);
        var response = await client.GetAsync("/health/live");
        Assert.Equal(expected, Assert.Single(response.Headers.GetValues("X-Correlation-ID")));
    }
}
