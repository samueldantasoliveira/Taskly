using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Rivulus.Domain.Entities;

public class TeamInvitation
{
    [BsonRepresentation(BsonType.String)] public Guid Id { get; private set; }
    [BsonRepresentation(BsonType.String)] public Guid TeamId { get; private set; }
    public string Email { get; private set; } = null!;
    public string TokenHash { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? AcceptedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public bool IsPending => AcceptedAt == null && RevokedAt == null && ExpiresAt > DateTime.UtcNow;
    protected TeamInvitation() { }
    public TeamInvitation(Guid teamId, string email, string tokenHash, DateTime expiresAt)
    {
        Id = Guid.NewGuid(); TeamId = teamId; Email = email.Trim().ToLowerInvariant();
        TokenHash = tokenHash; ExpiresAt = expiresAt; CreatedAt = DateTime.UtcNow;
    }
    public void Accept() => AcceptedAt = DateTime.UtcNow;
    public void Revoke() => RevokedAt = DateTime.UtcNow;
}
