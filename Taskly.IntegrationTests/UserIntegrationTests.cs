using System.Net;
using System.Net.Http.Json;

namespace Taskly.IntegrationTests;

public class UserIntegrationTests : IClassFixture<TasklyApiFactory>
{
    private readonly HttpClient _client;

    public UserIntegrationTests(TasklyApiFactory factory)
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

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        var user = new
        {
            Name = "João",
            Email = $"joao-{Guid.NewGuid()}@test.com",
            Password = "123456"
        };

        // Act
        var firstResponse = await _client.PostAsJsonAsync("/api/user", user);
        var secondResponse = await _client.PostAsJsonAsync("/api/user", user);

        // Assert
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);

    }
}
