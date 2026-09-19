using Microsoft.Extensions.Options;
using PartnerCommissions.Users.Api.Hosting;
using PartnerCommissions.Users.Api.Domain;
using PartnerCommissions.Users.Api.Infrastructure.Models;

namespace PartnerCommissions.Users.Api.Application;

public sealed class UsersService(
    IUserRepository users,
    IOptions<UsersOptions> options,
    ILogger<UsersService> logger,
    IUsersMetrics metrics) : IUsersService
{
    private readonly int _maxTreeDepth = options.Value.MaxTreeDepth;

    public async Task<Guid> CreateAsync(Guid? inviterId, CancellationToken cancellationToken)
    {
        if (inviterId is not null)
        {
            var inviter = inviterId.Value;
            await RequireAsync(inviter, cancellationToken);
            var chain = await users.GetInvitersAsync(inviter, _maxTreeDepth, cancellationToken);
            if (chain.Count >= _maxTreeDepth)
            {
                throw new TreeDepthExceededException(_maxTreeDepth, inviter);
            }
        }

        var user = new User { InviterId = inviterId };
        await users.AddAsync(user, cancellationToken);
        await users.SaveChangesAsync(cancellationToken);
        logger.UserCreated(user.ExternalId, user.InviterId);
        metrics.UserCreated();
        return user.ExternalId;
    }

    public async Task<IReadOnlyList<InviterTreeNode>> GetTreeAsync(
        Guid externalId,
        TreeDirection direction,
        CancellationToken cancellationToken)
    {
        await RequireAsync(externalId, cancellationToken);
        return await users.GetTreeAsync(externalId, direction, _maxTreeDepth, cancellationToken);
    }

    public async Task<IReadOnlyList<Inviter>> GetInvitersAsync(Guid externalId, CancellationToken cancellationToken)
    {
        await RequireAsync(externalId, cancellationToken);
        return await users.GetInvitersAsync(externalId, _maxTreeDepth, cancellationToken);
    }

    private async Task RequireAsync(Guid externalId, CancellationToken cancellationToken)
    {
        if (!await users.ExistsAsync(externalId, cancellationToken))
        {
            throw new UserNotFoundException(externalId);
        }
    }
}
