using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace Taskly.IntegrationTests;

public class AuthenticationIntegrationTests : IClassFixture<TasklyApiFactory>
{
    private readonly HttpClient _client;

    public AuthenticationIntegrationTests(TasklyApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOk()
    {
        // Arrange
        var email = $"joao-{Guid.NewGuid()}@test.com";
        var password = "123456";

        var user = new
        {
            Name = "João",
            Email = email,
            Password = password
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/user", user);
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var login = new
        {
            Email = email,
            Password = password
        };

        // Act
        var loginResponse =
            await _client.PostAsJsonAsync("/api/login", login);

        // Assert
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var response =
            await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();

        Assert.NotNull(response);
        Assert.False(string.IsNullOrWhiteSpace(response.Token));
        Assert.NotEqual(default, response.ExpiresAt);
        Assert.NotNull(response.User);
        Assert.Equal(user.Name, response.User.Name);
        Assert.Equal(user.Email, response.User.Email);

    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var email = $"joao-{Guid.NewGuid()}@test.com";
        var password = "123456";

        var user = new
        {
            Name = "João",
            Email = email,
            Password = password
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/user", user);
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var login = new
        {
            Email = email,
            Password = "654321"
        };

        // Act
        var loginResponse =
            await _client.PostAsJsonAsync("/api/login", login);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);

    }

    [Fact]
    public async Task Login_UserNotFound_ReturnsUnauthorized()
    {
        // Arrange

        var login = new
        {
            Email = "Pedro@02.com",
            Password = "123456"
        };

        // Act
        var loginResponse =
            await _client.PostAsJsonAsync("/api/login", login);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var deleteResponse = 
            await _client.DeleteAsync($"/api/user/{userId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_WithValidToken_ReturnsNoContent()
    {
        // Arrange
        var email = $"joao-{Guid.NewGuid()}@test.com";
        var password = "123456";

        var user = new
        {
            Name = "João",
            Email = email,
            Password = password
        };

        var registerResponse = await _client.PostAsJsonAsync("api/user", user);
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var createdUser =
        await registerResponse.Content.ReadFromJsonAsync<UserResponseDto>();

        Assert.NotNull(createdUser);

        var login = new
        {
            Email = email,
            Password = password
        };

        var loginResponse =
            await _client.PostAsJsonAsync("/api/login", login);

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginResult =
            await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();

        Assert.NotNull(loginResult);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginResult.Token);

        // Act
        var deleteResponse = 
            await _client.DeleteAsync($"/api/user/{createdUser.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        
        
    }
}