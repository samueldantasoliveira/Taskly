using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Taskly.IntegrationTests;

public class CorsIntegrationTests : IClassFixture<TasklyApiFactory>
{
    private readonly HttpClient _client;

    public CorsIntegrationTests(TasklyApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Preflight_AllowedOrigin_ReturnsCorsHeaders()
    {
        using var request = CreatePreflightRequest(
            TasklyApiFactory.AllowedOrigin
        );

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(
            TasklyApiFactory.AllowedOrigin,
            response.Headers.GetValues("Access-Control-Allow-Origin").Single()
        );
        Assert.Contains(
            "POST",
            response.Headers.GetValues("Access-Control-Allow-Methods")
                .Single()
        );

        var allowedHeaders = response.Headers
            .GetValues("Access-Control-Allow-Headers")
            .Single();

        Assert.Contains("authorization", allowedHeaders);
        Assert.Contains("content-type", allowedHeaders);
    }

    [Fact]
    public async Task Preflight_DisallowedOrigin_DoesNotReturnCorsHeaders()
    {
        using var request = CreatePreflightRequest(
            "https://untrusted.example"
        );

        var response = await _client.SendAsync(request);

        Assert.False(
            response.Headers.Contains("Access-Control-Allow-Origin")
        );
    }

    private static HttpRequestMessage CreatePreflightRequest(string origin)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Options,
            "/api/login"
        );

        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "POST");
        request.Headers.Add(
            "Access-Control-Request-Headers",
            "authorization,content-type"
        );

        return request;
    }
}

public class CorsConfigurationTests
{
    [Fact]
    public void Startup_EmptyAllowedOrigins_ThrowsValidationException()
    {
        using var factory = CreateFactory();

        var exception = Assert.Throws<OptionsValidationException>(
            () => factory.CreateClient()
        );

        Assert.Contains(
            "Cors:AllowedOrigins must contain at least one origin.",
            exception.Failures
        );
    }

    [Fact]
    public void Startup_InvalidAllowedOrigin_ThrowsValidationException()
    {
        using var factory = CreateFactory("https://taskly.test/path");

        var exception = Assert.Throws<OptionsValidationException>(
            () => factory.CreateClient()
        );

        Assert.Contains(
            "Cors:AllowedOrigins must contain only valid HTTP or HTTPS origins.",
            exception.Failures
        );
    }

    private static WebApplicationFactory<Program> CreateFactory(
        string? allowedOrigin = null
    )
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("ConfigurationTests");

                builder.ConfigureAppConfiguration((context, config) =>
                {
                    var settings = new Dictionary<string, string?>
                    {
                        ["AllowedHosts"] = "localhost",
                        ["Jwt:Key"] =
                            "J2u7k1sH+H1i5hE0zS1V1bW9YbUzwr9vN7oVPy7QxPE=",
                        ["MongoDb:ConnectionString"] =
                            "mongodb://localhost:27018",
                        ["MongoDb:DatabaseName"] = "ConfigurationTests"
                    };

                    if (allowedOrigin is not null)
                    {
                        settings["Cors:AllowedOrigins:0"] = allowedOrigin;
                    }

                    config.AddInMemoryCollection(settings);
                });
            });
    }
}
