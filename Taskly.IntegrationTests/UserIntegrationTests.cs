using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace Taskly.IntegrationTests;

public class UserIntegrationTests : IClassFixture<TasklyApiFactory>
{
    private readonly HttpClient _client;

    public UserIntegrationTests(TasklyApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        // Arrange
        var user = new
        {
            Name = "João",
            Email = $"joao-{Guid.NewGuid()}@test.com",
            Password = "123456"
        };
        var duplicateWithDifferentCasing = new
        {
            user.Name,
            Email = user.Email.ToUpperInvariant(),
            user.Password
        };

        // Act
        var firstResponse = 
            await _client.PostAsJsonAsync("/api/user", user);
        var secondResponse = 
            await _client.PostAsJsonAsync("/api/user", duplicateWithDifferentCasing);

        // Assert
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);

    }

    [Fact]
    public async Task UpdateUser_AnotherAuthenticatedUser_ReturnsForbidden()
    {
        // Arrange
        var password = "123456";

        var authenticatedUser = new
        {
            Name = "João",
            Email = $"joao-{Guid.NewGuid()}@test.com",
            Password = password
        };

        var targetUser = new
        {
            Name = "Maria",
            Email = $"maria-{Guid.NewGuid()}@test.com",
            Password = password
        };

        var authenticatedRegisterResponse =
            await _client.PostAsJsonAsync("/api/user", authenticatedUser);
        var targetRegisterResponse =
            await _client.PostAsJsonAsync("/api/user", targetUser);

        Assert.Equal(HttpStatusCode.OK, authenticatedRegisterResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, targetRegisterResponse.StatusCode);

        var target = await targetRegisterResponse.Content
            .ReadFromJsonAsync<UserResponseDto>();
        Assert.NotNull(target);

        var loginResponse = await _client.PostAsJsonAsync("/api/login", new
        {
            authenticatedUser.Email,
            authenticatedUser.Password
        });
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginResult = await loginResponse.Content
            .ReadFromJsonAsync<LoginResponseDto>();
        Assert.NotNull(loginResult);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginResult.Token);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/user/{target.Id}",
            new { Name = "Nome alterado indevidamente" });

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
