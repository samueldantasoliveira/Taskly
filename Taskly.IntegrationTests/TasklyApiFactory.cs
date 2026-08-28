using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Taskly.IntegrationTests;

public class TasklyApiFactory : WebApplicationFactory<Program>
{
    private const string ConnectionString = "mongodb://localhost:27018";

    public string DatabaseName { get; } =
        $"TasklyIntegrationTests_{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MongoDb:ConnectionString"] = ConnectionString,
                ["MongoDb:DatabaseName"] = DatabaseName
            });
        });
    }

    public override async ValueTask DisposeAsync()
    {
        var mongoClient = new MongoClient(ConnectionString);
        await mongoClient.DropDatabaseAsync(DatabaseName);
        await base.DisposeAsync();
    }
}
