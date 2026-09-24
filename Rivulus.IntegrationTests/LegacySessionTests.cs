using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using Rivulus.Application.DTOs;

namespace Rivulus.IntegrationTests;

public class LegacySessionTests(RivulusApiFactory factory) : IClassFixture<RivulusApiFactory>
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task LegacyAccount_RemainsAuthenticated_AndRevokesSessionAfterPasswordChangeOrDeletion(bool delete)
    {
        using var client = factory.CreateClient();
        using var scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<MongoClient>()
            .GetDatabase(factory.DatabaseName).GetCollection<BsonDocument>("Users");
        var id = Guid.NewGuid();
        var email = $"legacy-{id:N}@example.test";
        const string password = "OriginalPassword123!";
        // Seed the actual old schema: no SessionVersion or Version fields.
        var document = new BsonDocument
        {
            ["_id"] = id.ToString(), ["Name"] = "Legacy account", ["Email"] = email,
            ["PasswordHash"] = PasswordHasher.HashPassword(password),
            ["CreatedAt"] = DateTime.UtcNow, ["UpdatedAt"] = DateTime.UtcNow,
            ["DeletedAt"] = BsonNull.Value
        };
        await users.InsertOneAsync(document);
        var helper = new UserTestHelper(client);
        var session = await helper.LoginAsync(email, password);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.Token);
        for (var read = 0; read < 3; read++)
        {
            var response = await client.GetAsync("/api/user/me");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(id, (await response.Content.ReadFromJsonAsync<UserResponseDto>())!.Id);
        }
        // Reads must not require a migration or change the stored account.
        Assert.Equal(document, await users.Find(new BsonDocument("_id", id.ToString())).SingleAsync());

        var changed = delete
            ? await client.DeleteAsync($"/api/user/{id}")
            : await client.PutAsJsonAsync($"/api/user/{id}", new { Password = "NewPassword123!" });
        changed.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/user/me")).StatusCode);
        if (!delete)
        {
            var stored = await users.Find(new BsonDocument("_id", id.ToString())).SingleAsync();
            Assert.NotEqual("0", stored["SessionVersion"].AsString);
            client.DefaultRequestHeaders.Authorization = null;
            var newSession = await helper.LoginAsync(email, "NewPassword123!");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", newSession.Token);
            (await client.GetAsync("/api/user/me")).EnsureSuccessStatusCode();
        }
    }
}
