using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Taskly.IntegrationTests;

public class TasklyApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MongoDb:ConnectionString"] = "mongodb://localhost:27018",
                ["MongoDb:DatabaseName"] = "TasklyIntegrationTests"
            });
        });
    }
}