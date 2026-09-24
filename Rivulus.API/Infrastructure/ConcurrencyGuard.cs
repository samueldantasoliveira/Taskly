using MongoDB.Driver;
using Rivulus.Application.Results;

namespace Rivulus.Infrastructure;

internal static class ConcurrencyGuard
{
    public static FilterDefinition<T> WithVersion<T>(FilterDefinition<T> filter, long version)
    {
        var builder = Builders<T>.Filter;
        var versionFilter = builder.Eq("Version", version);
        // Documents saved before versioning are treated as version zero.
        if (version == 0)
            versionFilter |= builder.Exists("Version", false);
        return filter & versionFilter;
    }

    public static async Task<bool> CheckWrite<T>(IMongoCollection<T> collection,
        FilterDefinition<T> filter, long matchedCount, CancellationToken cancellationToken)
    {
        if (matchedCount > 0) return true;
        if (await collection.Find(filter).AnyAsync(cancellationToken))
            throw new ConcurrencyConflictException();
        return false;
    }
}
