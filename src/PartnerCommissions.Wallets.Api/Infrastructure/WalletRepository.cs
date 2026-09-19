using EFCore.PostgresExtensions.Enums;
using EFCore.PostgresExtensions.Extensions;
using Microsoft.EntityFrameworkCore;
using PartnerCommissions.Wallets.Api.Domain;
using PartnerCommissions.Wallets.Api.Infrastructure.Models;

namespace PartnerCommissions.Wallets.Api.Infrastructure;

internal sealed class WalletRepository(
    WalletsDbContext db,
    IUtcTime utcTime) : IWalletRepository
{
    public Task<decimal> GetBalanceAsync(Guid partnerExternalId, CancellationToken cancellationToken)
    {
        return db.Wallets
            .Where(x => x.PartnerExternalId == partnerExternalId)
            .Select(x => x.Balance)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PayoutRecord>> ListPayoutsAsync(
        Guid partnerExternalId,
        CancellationToken cancellationToken)
    {
        return await db.Payouts
            .Where(x => x.PartnerExternalId == partnerExternalId)
            .OrderByDescending(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .Select(x => new PayoutRecord(
                x.Id,
                x.CommissionId,
                x.PartnerExternalId,
                x.Amount,
                x.EventOperationId,
                x.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<CreditResult> CreditAsync(
        IReadOnlyList<UnpaidCommission> unpaid,
        CancellationToken cancellationToken)
    {
        if (unpaid.Count == 0)
        {
            return new CreditResult(0, 0);
        }

        var commissions = unpaid.DistinctBy(x => x.Id).ToList();
        var commissionIds = commissions.Select(x => x.Id).ToArray();

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var uncredited = await FindUncreditedAsync(commissions, commissionIds, cancellationToken);

        foreach (var partnerCommissions in uncredited.GroupBy(x => x.PartnerExternalId).OrderBy(x => x.Key))
        {
            var creditAmount = partnerCommissions.Sum(x => x.Amount);
            var partnerExternalId = partnerCommissions.Key;
            var wallet = await db.Wallets
                .Where(x => x.PartnerExternalId == partnerExternalId)
                .ForUpdate(LockBehavior.Default)
                .FirstOrDefaultAsync(cancellationToken);
            if (wallet is null)
            {
                await db.Wallets.AddAsync(
                    new Wallet { PartnerExternalId = partnerExternalId, Balance = creditAmount },
                    cancellationToken);
            }
            else
            {
                wallet.Balance += creditAmount;
            }
        }

        var now = utcTime.Now;

        if (uncredited.Count > 0)
        {
            await db.Payouts.AddRangeAsync(
                uncredited.Select(x => new Payout
                {
                    CommissionId = x.Id,
                    PartnerExternalId = x.PartnerExternalId,
                    Amount = x.Amount,
                    EventOperationId = x.EventOperationId,
                    CreatedAt = now
                }),
                cancellationToken);
        }

        await db.OutboxMessages.AddAsync(
            new OutboxMessage
            {
                CommissionIds = commissionIds,
                CreatedAt = now
            },
            cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new CreditResult(uncredited.Count, commissionIds.Length - uncredited.Count);
    }

    public async Task<IReadOnlyList<PendingAck>> ClaimOutboxAsync(
        int limit,
        TimeSpan claimTimeout,
        CancellationToken cancellationToken)
    {
        var now = utcTime.Now;
        var availableAfter = now + claimTimeout;

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var claimed = await db.OutboxMessages
            .Where(x => x.ProcessedAt == null && (x.AvailableAfter == null || x.AvailableAfter < now))
            .OrderBy(x => x.Id)
            .Take(limit)
            .ForUpdate(LockBehavior.SkipLocked)
            .ToListAsync(cancellationToken);

        if (claimed.Count == 0)
        {
            await transaction.CommitAsync(cancellationToken);
            return [];
        }

        var pending = new List<PendingAck>(claimed.Count);
        foreach (var message in claimed)
        {
            message.AvailableAfter = availableAfter;
            pending.Add(new PendingAck(message.Id, message.CommissionIds));
        }

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return pending;
    }

    public Task MarkOutboxProcessedAsync(IReadOnlyList<Guid> ids, CancellationToken cancellationToken)
    {
        return db.OutboxMessages
            .Where(x => ids.Contains(x.Id) && x.ProcessedAt == null)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.ProcessedAt, utcTime.Now)
                    .SetProperty(x => x.AvailableAfter, (DateTimeOffset?)null),
                cancellationToken);
    }

    public void ClearTracked()
    {
        db.ChangeTracker.Clear();
    }

    private async Task<List<UnpaidCommission>> FindUncreditedAsync(
        List<UnpaidCommission> commissions,
        Guid[] commissionIds,
        CancellationToken cancellationToken)
    {
        var alreadyPaid = await db.Payouts
            .Where(x => commissionIds.Contains(x.CommissionId))
            .Select(x => x.CommissionId)
            .ToHashSetAsync(cancellationToken);
        return commissions.FindAll(x => !alreadyPaid.Contains(x.Id));
    }
}
