using PartnerCommissions.Users.Api.Infrastructure.Models;

namespace PartnerCommissions.Users.Api.Domain;

public interface IUserRepository
{
    Task<bool> ExistsAsync(Guid externalId, CancellationToken cancellationToken);
    Task AddAsync(User user, CancellationToken cancellationToken);
    Task<IReadOnlyList<Inviter>> GetInvitersAsync(Guid externalId, int maxDepth, CancellationToken cancellationToken);
    Task<IReadOnlyList<InviterTreeNode>> GetTreeAsync(Guid externalId, TreeDirection direction, int maxDepth, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
