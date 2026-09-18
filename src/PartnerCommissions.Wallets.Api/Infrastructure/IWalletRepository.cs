using PartnerCommissions.Wallets.Domain;

namespace PartnerCommissions.Wallets.Infrastructure;

public interface IWalletRepository
{
    Task<decimal> GetBalanceAsync(Guid partnerExternalId, CancellationToken cancellationToken);

    Task<IReadOnlyList<PayoutRecord>> ListPayoutsAsync(Guid partnerExternalId, CancellationToken cancellationToken);

    Task<CreditResult> CreditAsync(IReadOnlyList<UnpaidCommission> unpaid, CancellationToken cancellationToken);

    Task<IReadOnlyList<PendingAck>> ClaimOutboxAsync(int limit, TimeSpan claimTimeout, CancellationToken cancellationToken);

    Task MarkOutboxProcessedAsync(IReadOnlyList<Guid> ids, CancellationToken cancellationToken);

    void ClearTracked();
}
