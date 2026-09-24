using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Rivulus.Infrastructure;

public sealed class MongoDbHealthCheck(
    MongoClient client,
    IOptions<MongoDbSettings> mongoDbOptions
) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        var database = client.GetDatabase(mongoDbOptions.Value.DatabaseName);

        await database.RunCommandAsync<BsonDocument>(
            new BsonDocument("ping", 1),
            cancellationToken: cancellationToken
        );

        return HealthCheckResult.Healthy();
    }
}
