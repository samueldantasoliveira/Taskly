using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
public class UserTestHelper
{
    private readonly HttpClient _client;

    public UserTestHelper(HttpClient client)
    {
        _client = client;
    }

    public async Task<UserResponseDto> CreateUserAsync()
    {
        var user = new
        {
            Name = "Test User",
            Email = $"test-{Guid.NewGuid()}@test.com",
            Password = "123456"
        };

        var response =
            await _client.PostAsJsonAsync("/api/user", user);

        response.EnsureSuccessStatusCode();

        return (await response.Content
            .ReadFromJsonAsync<UserResponseDto>())!;
    }

    public async Task<LoginResponseDto> LoginAsync(
        string email,
        string password)
    {
        var login = new
        {
            Email = email,
            Password = password
        };

        var response =
            await _client.PostAsJsonAsync("/api/login", login);

        response.EnsureSuccessStatusCode();

        return (await response.Content
            .ReadFromJsonAsync<LoginResponseDto>())!;
    }

    public async Task<LoginResponseDto> CreateUserAndLoginAsync()
    {
        var email = $"test-{Guid.NewGuid()}@test.com";
        var password = "123456";

        var user = new
        {
            Name = "Test User",
            Email = email,
            Password = password
        };

        var registerResponse =
            await _client.PostAsJsonAsync("/api/user", user);

        registerResponse.EnsureSuccessStatusCode();

        var login = new
        {
            Email = email,
            Password = password
        };

        var loginResponse =
            await _client.PostAsJsonAsync("/api/login", login);

        loginResponse.EnsureSuccessStatusCode();

        return (await loginResponse.Content
            .ReadFromJsonAsync<LoginResponseDto>())!;
    }
}