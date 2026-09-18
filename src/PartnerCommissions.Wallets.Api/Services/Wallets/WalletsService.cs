using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using PartnerCommissions.Wallets.Api.Logging;
using PartnerCommissions.Wallets.Api.Options;
using PartnerCommissions.Wallets.Domain;
using PartnerCommissions.Wallets.Infrastructure;

namespace PartnerCommissions.Wallets.Api.Services.Wallets;

public sealed class WalletsService(
    IWalletRepository wallets,
    ICommissionPayouts payouts,
    IOptions<WalletsOptions> options,
    ILogger<WalletsService> logger) : IWalletsService
{
    public Task<decimal> GetBalanceAsync(Guid partnerExternalId, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(partnerExternalId, Guid.Empty);
        return wallets.GetBalanceAsync(partnerExternalId, cancellationToken);
    }

    public Task<IReadOnlyList<PayoutRecord>> ListPayoutsAsync(
        Guid partnerExternalId,
        CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(partnerExternalId, Guid.Empty);
        return wallets.ListPayoutsAsync(partnerExternalId, cancellationToken);
    }

    public async Task<PayoutRunResult> RunPayoutsAsync(CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var acked = await DrainOutboxAsync(settings.BatchLimit, settings.AckRetry, cancellationToken);
        var unpaid = await payouts.GetUnpaidAsync(settings.BatchLimit, (int)settings.ClaimTimeout.TotalSeconds, cancellationToken);
        if (unpaid.Count == 0)
        {
            logger.PayoutRunCompleted(0, 0, acked);
            return new PayoutRunResult(0, 0, acked);
        }

        var credit = await CreditWithRetryAsync(unpaid, cancellationToken);
        acked += await DrainOutboxAsync(settings.BatchLimit, settings.AckRetry, cancellationToken);
        logger.PayoutRunCompleted(credit.Credited, credit.AlreadyPaid, acked);
        return new PayoutRunResult(credit.Credited, credit.AlreadyPaid, acked);
    }

    private async Task<CreditResult> CreditWithRetryAsync(
        IReadOnlyList<UnpaidCommission> unpaid,
        CancellationToken cancellationToken)
    {
        var maxAttempts = options.Value.MaxCreditAttempts;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return await wallets.CreditAsync(unpaid, cancellationToken);
            }
            catch (DbUpdateException exception) when (exception.InnerException is PostgresException 
                  { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                wallets.ClearTracked();
                if (attempt == maxAttempts)
                {
                    throw new WalletCreditConflictException(maxAttempts, exception);
                }

                logger.CreditConflict(attempt, maxAttempts);
            }
        }

        throw new WalletCreditConflictException(maxAttempts);
    }

    private async Task<int> DrainOutboxAsync(
        int limit,
        TimeSpan ackRetry,
        CancellationToken cancellationToken)
    {
        var pending = await wallets.ClaimOutboxAsync(limit, ackRetry, cancellationToken);
        if (pending.Count == 0)
        {
            return 0;
        }

        var processed = new List<Guid>(pending.Count);
        var acked = 0;
        foreach (var message in pending)
        {
            try
            {
                await payouts.AckPaidAsync(message.CommissionIds, cancellationToken);
                processed.Add(message.Id);
                acked += message.CommissionIds.Count;
            }
            catch (CommissionsUnavailableException exception)
            {
                logger.AckFailed(exception, message.Id, message.CommissionIds.Count);
            }
        }

        if (processed.Count > 0)
        {
            await wallets.MarkOutboxProcessedAsync(processed, cancellationToken);
            logger.PaidAcked(processed.Count, acked);
        }

        return acked;
    }
}
