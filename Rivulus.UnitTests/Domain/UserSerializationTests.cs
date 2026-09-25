using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Rivulus.Domain.Entities;

namespace Rivulus.Tests.Domain;

public class UserSerializationTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RepeatedReads_PreserveSessionAndStoredFields(bool legacy)
    {
        var user = new User("Legacy user", "legacy@example.test", "synthetic-hash");
        var document = user.ToBsonDocument();
        if (legacy)
        {
            document.Remove("SessionVersion");
            document.Remove("Version");
        }

        var first = BsonSerializer.Deserialize<User>(document);
        var second = BsonSerializer.Deserialize<User>(document);

        Assert.Equal(legacy ? "0" : user.SessionVersion, first.SessionVersion);
        Assert.Equal(first.SessionVersion, second.SessionVersion);
        Assert.Equal(user.Id, first.Id);
        Assert.Equal(user.Email, first.Email);
        Assert.Equal(user.Name, first.Name);
        Assert.Equal(user.PasswordHash, first.PasswordHash);
        Assert.Equal(document["CreatedAt"].ToUniversalTime(), first.CreatedAt);
        Assert.Equal(document["UpdatedAt"].ToUniversalTime(), first.UpdatedAt);
        Assert.Null(first.DeletedAt);
        Assert.Null(first.AvatarKey);
    }
}
