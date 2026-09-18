using PartnerCommissions.Wallets.Domain;

namespace PartnerCommissions.Wallets.Api.Services.Wallets;

public interface IWalletsService
{
    Task<decimal> GetBalanceAsync(Guid partnerExternalId, CancellationToken cancellationToken);

    Task<IReadOnlyList<PayoutRecord>> ListPayoutsAsync(
        Guid partnerExternalId,
        CancellationToken cancellationToken);

    Task<PayoutRunResult> RunPayoutsAsync(CancellationToken cancellationToken);
}
