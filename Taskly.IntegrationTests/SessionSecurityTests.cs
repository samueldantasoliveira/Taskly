using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Taskly.IntegrationTests;

public class SessionSecurityTests(TasklyApiFactory factory) : IClassFixture<TasklyApiFactory>
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PasswordChangeOrDeletion_RejectsPreviouslyIssuedToken(bool delete)
    {
        using var client = factory.CreateClient();
        var helper = new UserTestHelper(client);
        var login = await helper.CreateUserAndLoginAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);
        var response = delete
            ? await client.DeleteAsync($"/api/user/{login.User.Id}")
            : await client.PutAsJsonAsync($"/api/user/{login.User.Id}", new { Password = "NewPassword123!" });
        response.EnsureSuccessStatusCode();
        // This endpoint previously accepted the token without looking up its user.
        var protectedResponse = await client.PostAsJsonAsync("/api/team", new { Name = "Blocked" });
        Assert.Equal(HttpStatusCode.Unauthorized, protectedResponse.StatusCode);
        if (!delete)
        {
            client.DefaultRequestHeaders.Authorization = null;
            var newSession = await helper.LoginAsync(login.User.Email, "NewPassword123!");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", newSession.Token);
            (await client.GetAsync("/api/user/me")).EnsureSuccessStatusCode();
        }
    }

    [Theory]
    [InlineData(5)]
    [InlineData(129)]
    public async Task PasswordLength_IsValidatedForRegistrationAndUpdate(int length)
    {
        using var client = factory.CreateClient();
        var login = await new UserTestHelper(client).CreateUserAndLoginAsync();
        var password = new string('x', length);
        var create = await client.PostAsJsonAsync("/api/user", new
            { Name = "Invalid", Email = $"{Guid.NewGuid()}@test.com", Password = password });
        Assert.Equal(HttpStatusCode.BadRequest, create.StatusCode);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);
        var update = await client.PutAsJsonAsync($"/api/user/{login.User.Id}", new { Password = password });
        Assert.Equal(HttpStatusCode.BadRequest, update.StatusCode);
        (await client.GetAsync("/api/user/me")).EnsureSuccessStatusCode();
    }

    [Theory]
    [InlineData("login")]
    [InlineData("user")]
    public async Task AuthenticationEndpoints_RejectExcessAttemptsWithRetryAfter(string endpoint)
    {
        await using var limited = factory.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, config) =>
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AuthenticationLimits:LoginPerMinute"] = "2",
                ["AuthenticationLimits:RegistrationPerMinute"] = "2"
            })));
        using var client = limited.CreateClient();
        for (var i = 0; i < 2; i++)
        {
            var response = await client.PostAsJsonAsync($"/api/{endpoint}", new
                { Name = "Invalid", Email = "invalid", Password = "invalid" });
            Assert.NotEqual(HttpStatusCode.TooManyRequests, response.StatusCode);
        }
        var rejected = await client.PostAsJsonAsync($"/api/{endpoint}", new
            { Name = "Invalid", Email = "invalid", Password = "invalid" });
        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
        Assert.NotNull(rejected.Headers.RetryAfter);
    }
}
