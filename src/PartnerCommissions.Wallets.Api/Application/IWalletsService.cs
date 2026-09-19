using PartnerCommissions.Wallets.Api.Domain;

namespace PartnerCommissions.Wallets.Api.Application;

public interface IWalletsService
{
    Task<decimal> GetBalanceAsync(Guid partnerExternalId, CancellationToken cancellationToken);

    Task<IReadOnlyList<PayoutRecord>> ListPayoutsAsync(
        Guid partnerExternalId,
        CancellationToken cancellationToken);

    Task<PayoutRunResult> RunPayoutsAsync(CancellationToken cancellationToken);
}
