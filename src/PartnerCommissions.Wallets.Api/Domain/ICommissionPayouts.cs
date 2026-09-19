namespace PartnerCommissions.Wallets.Api.Domain;

public interface ICommissionPayouts
{
    Task<IReadOnlyList<UnpaidCommission>> GetUnpaidAsync(
        int limit,
        int claimTimeoutSeconds,
        CancellationToken cancellationToken);

    Task AckPaidAsync(IReadOnlyList<Guid> ids, CancellationToken cancellationToken);
}
