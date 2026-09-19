using Microsoft.EntityFrameworkCore;
using PartnerCommissions.Users.Api.Domain;
using PartnerCommissions.Users.Api.Infrastructure.Models;

namespace PartnerCommissions.Users.Api.Infrastructure;

internal sealed class UserRepository(UsersDbContext db) : IUserRepository
{
    public Task<bool> ExistsAsync(Guid externalId, CancellationToken cancellationToken)
    {
        return db.Users.AnyAsync(x => x.ExternalId == externalId, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        await db.Users.AddAsync(user, cancellationToken);
    }

    public async Task<IReadOnlyList<Inviter>> GetInvitersAsync(
        Guid externalId,
        int maxDepth,
        CancellationToken cancellationToken)
    {
        return await db.GetInviterTree(externalId, maxDepth, "up")
            .Where(x => x.Level > 0)
            .OrderBy(x => x.Level)
            .Select(x => new Inviter(x.ExternalId, x.Level))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<InviterTreeNode>> GetTreeAsync(
        Guid externalId,
        TreeDirection direction,
        int maxDepth,
        CancellationToken cancellationToken)
    {
        var dir = direction == TreeDirection.Up ? "up" : "down";
        return await db.GetInviterTree(externalId, maxDepth, dir)
            .OrderBy(x => x.Level)
            .ThenBy(x => x.ExternalId)
            .Select(x => new InviterTreeNode(x.ExternalId, x.InviterId, x.Level))
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return db.SaveChangesAsync(cancellationToken);
    }
}
