using PartnerCommissions.Users.Api.Domain;

namespace PartnerCommissions.Users.Api.Application;

public interface IUsersService
{
    Task<Guid> CreateAsync(Guid? inviterId, CancellationToken cancellationToken);
    Task<IReadOnlyList<InviterTreeNode>> GetTreeAsync(Guid externalId, TreeDirection direction, CancellationToken cancellationToken);
    Task<IReadOnlyList<Inviter>> GetInvitersAsync(Guid externalId, CancellationToken cancellationToken);
}
