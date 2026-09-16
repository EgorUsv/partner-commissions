using PartnerCommissions.Users.Domain;

namespace PartnerCommissions.Users.Api.Services;

public interface IUsersAppService
{
    Task<Guid> CreateAsync(Guid? inviterId, CancellationToken cancellationToken);
    Task<IReadOnlyList<InviterTreeNode>> GetTreeAsync(Guid externalId, TreeDirection direction, CancellationToken cancellationToken);
    Task<IReadOnlyList<Inviter>> GetInvitersAsync(Guid externalId, CancellationToken cancellationToken);
}
