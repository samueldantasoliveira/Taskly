using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace Taskly.IntegrationTests;

public class UserIntegrationTests : IClassFixture<TasklyApiFactory>
{
    private readonly HttpClient _client;
    private readonly UserTestHelper _userHelper;

    public UserIntegrationTests(TasklyApiFactory factory)
    {
        _client = factory.CreateClient();
        _userHelper = new UserTestHelper(_client);
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

    [Fact]
    public async Task GetCurrentUser_AuthenticatedUser_ReturnsUser()
    {
        var login = await _userHelper.CreateUserAndLoginAsync();
        SetBearerToken(login.Token);

        var response = await _client.GetAsync("/api/user/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<UserResponseDto>();
        Assert.NotNull(user);
        Assert.Equal(login.User.Id, user.Id);
        Assert.Equal(login.User.Name, user.Name);
        Assert.Equal(login.User.Email, user.Email);
    }

    [Fact]
    public async Task GetCurrentUser_WithoutAuthentication_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/user/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetCurrentUser_DeletedUser_ReturnsNotFound()
    {
        var login = await _userHelper.CreateUserAndLoginAsync();
        SetBearerToken(login.Token);
        var deleteResponse = await _client.DeleteAsync(
            $"/api/user/{login.User.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var response = await _client.GetAsync("/api/user/me");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SearchUser_AuthenticatedAndEmailExists_ReturnsMinimalUserData()
    {
        var login = await _userHelper.CreateUserAndLoginAsync();
        SetBearerToken(login.Token);
        var queryEmail = Uri.EscapeDataString(
            $"  {login.User.Email.ToUpperInvariant()}  ");

        var response = await _client.GetAsync(
            $"/api/user/search?email={queryEmail}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<UserResponseDto>();
        Assert.NotNull(user);
        Assert.Equal(login.User.Id, user.Id);
        Assert.Equal(login.User.Name, user.Name);
        Assert.Equal(login.User.Email, user.Email);

        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("password", responseBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", responseBody, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SearchUser_EmailDoesNotExist_ReturnsNotFound()
    {
        var login = await _userHelper.CreateUserAndLoginAsync();
        SetBearerToken(login.Token);

        var response = await _client.GetAsync(
            $"/api/user/search?email=missing-{Guid.NewGuid()}%40test.com");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("%20%20%20")]
    [InlineData("invalid-email")]
    public async Task SearchUser_InvalidEmail_ReturnsBadRequest(string email)
    {
        var login = await _userHelper.CreateUserAndLoginAsync();
        SetBearerToken(login.Token);

        var response = await _client.GetAsync(
            $"/api/user/search?email={email}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SearchUser_WithoutAuthentication_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync(
            "/api/user/search?email=user%40test.com");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SearchUser_DeletedUser_ReturnsNotFound()
    {
        var login = await _userHelper.CreateUserAndLoginAsync();
        SetBearerToken(login.Token);
        var deleteResponse = await _client.DeleteAsync(
            $"/api/user/{login.User.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var response = await _client.GetAsync(
            $"/api/user/search?email={Uri.EscapeDataString(login.User.Email)}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private void SetBearerToken(string token)
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }
}
