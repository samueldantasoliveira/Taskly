using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Taskly.IntegrationTests;

public class TasklyApiFactory : WebApplicationFactory<Program>
{
    public const string AllowedOrigin = "https://frontend.taskly.test";

    public string DatabaseName { get; } =
        $"TasklyIntegrationTests_{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MongoDb:DatabaseName"] = DatabaseName
            });
        });
    }

    public override async ValueTask DisposeAsync()
    {
        var mongoClient = Services.GetRequiredService<MongoClient>();
        await mongoClient.DropDatabaseAsync(DatabaseName);
        await base.DisposeAsync();
    }
}
