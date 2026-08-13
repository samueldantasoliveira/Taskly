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
        var response = 
            await _client.PostAsJsonAsync("/api/user", user);


        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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

        // Act
        var firstResponse = 
            await _client.PostAsJsonAsync("/api/user", user);
        var secondResponse = 
            await _client.PostAsJsonAsync("/api/user", user);

        // Assert
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);

    }

    [Fact]
    public async Task UpdateUser_ValidData_ReturnsOk()
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

        var registerResponse = 
            await _client.PostAsJsonAsync("/api/user", user);
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

        var updatedUser = new
        {
            Name = "Pedro" 
        };

        // Act
        var response = 
            await _client.PutAsJsonAsync($"/api/user/{loginResult.User.Id}", updatedUser);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updatedUserResponse =
            await response.Content.ReadFromJsonAsync<UserResponseDto>();

        Assert.NotNull(updatedUserResponse);
        Assert.Equal("Pedro", updatedUserResponse.Name);
    }

    [Fact]
    public async Task UpdateUser_DuplicateEmail_ReturnsConflict()
    {
         // Arrange
        var email = $"joao-{Guid.NewGuid()}@test.com";
        var password = "123456";

        var firstUser = new
        {
            Name = "João",
            Email = email,
            Password = password
        };

        var secondUser = new
        {
            Name = "Maria",
            Email = $"Maria-{Guid.NewGuid()}@test.com",
            Password = password
        };

        var firstRegisterResponse = 
            await _client.PostAsJsonAsync("/api/user", firstUser);
        var secondRegisterResponse = 
            await _client.PostAsJsonAsync("/api/user", secondUser);

        Assert.Equal(HttpStatusCode.OK, firstRegisterResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, secondRegisterResponse.StatusCode);

        var firstCreatedUser =
            await firstRegisterResponse.Content.ReadFromJsonAsync<UserResponseDto>();
        var secondCreatedUser =
            await secondRegisterResponse.Content.ReadFromJsonAsync<UserResponseDto>();

        Assert.NotNull(firstCreatedUser);
        Assert.NotNull(secondCreatedUser);

        var login = new
        {
            secondUser.Email,
            secondUser.Password
        };

        var loginResponse = 
            await _client.PostAsJsonAsync("/api/login", login);

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginResult =
            await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();

        Assert.NotNull(loginResult);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginResult.Token);

        var updatedUser = new
        {
            firstUser.Email
        };

        // Act
        var response = 
            await _client.PutAsJsonAsync($"/api/user/{loginResult.User.Id}", updatedUser);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict , response.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_ExistingUser_ReturnsNoContent()
    {
        // Arrange
        var email = $"joao-{Guid.NewGuid()}@test.com";
        var password = "123456";
        
        var user = new
        {
            Name = "João",
            email,
            password
        };

        var registerResponse = 
            await _client.PostAsJsonAsync("api/user", user);
        Assert.Equal(HttpStatusCode.OK , registerResponse.StatusCode);

        var createdUser =
            await registerResponse.Content.ReadFromJsonAsync<UserResponseDto>();
        
        Assert.NotNull(createdUser);

        var login = new
        {
            email,
            password
        };

        var loginResponse = 
            await _client.PostAsJsonAsync("api/login", login);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginResult =
            await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.NotNull(loginResult);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginResult.Token);

        // Act
        var response = await _client.DeleteAsync($"api/user/{createdUser.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent , response.StatusCode);
    }
}
