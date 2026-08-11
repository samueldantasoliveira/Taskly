using System.Net;
using System.Net.Http.Json;

namespace Taskly.IntegrationTests;

public class AuthenticationIntegrationTests : IClassFixture<TasklyApiFactory>
{
    private readonly HttpClient _client;

    public AuthenticationIntegrationTests(TasklyApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ValidUser_ReturnsOk()
    {
        // Arrange
        var user = new
        {
            Name = "João",
            Email = $"joao-{Guid.NewGuid()}@test.com",
            Password = "123456"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/user", user);


        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}