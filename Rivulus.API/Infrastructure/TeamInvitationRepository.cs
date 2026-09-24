using MongoDB.Driver;
using Rivulus.Application;
using Rivulus.Domain.Entities;
namespace Rivulus.Infrastructure;
public class TeamInvitationRepository(MongoDbContext context) : ITeamInvitationRepository
{
    public Task AddAsync(TeamInvitation value, CancellationToken ct = default) => context.TeamInvitations.InsertOneAsync(value, cancellationToken: ct);
    public Task<TeamInvitation?> GetByTokenHashAsync(string hash, CancellationToken ct = default) => context.TeamInvitations.Find(x => x.TokenHash == hash).FirstOrDefaultAsync(ct)!;
    public Task<TeamInvitation?> GetByIdAsync(Guid id, CancellationToken ct = default) => context.TeamInvitations.Find(x => x.Id == id).FirstOrDefaultAsync(ct)!;
    public Task<List<TeamInvitation>> GetPendingByTeamIdAsync(Guid teamId, CancellationToken ct = default) => context.TeamInvitations.Find(x => x.TeamId == teamId && x.AcceptedAt == null && x.RevokedAt == null && x.ExpiresAt > DateTime.UtcNow).SortByDescending(x => x.CreatedAt).ToListAsync(ct);
    public Task<bool> HasPendingAsync(Guid teamId, string email, CancellationToken ct = default) => context.TeamInvitations.Find(x => x.TeamId == teamId && x.Email == email && x.AcceptedAt == null && x.RevokedAt == null && x.ExpiresAt > DateTime.UtcNow).AnyAsync(ct);
    public Task UpdateAsync(TeamInvitation value, CancellationToken ct = default) => context.TeamInvitations.ReplaceOneAsync(x => x.Id == value.Id, value, cancellationToken: ct);
}
