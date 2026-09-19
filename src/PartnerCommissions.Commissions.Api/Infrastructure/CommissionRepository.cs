using EFCore.PostgresExtensions.Enums;
using EFCore.PostgresExtensions.Extensions;
using Microsoft.EntityFrameworkCore;
using PartnerCommissions.Commissions.Api.Domain;
using PartnerCommissions.Commissions.Api.Infrastructure.Models;

namespace PartnerCommissions.Commissions.Api.Infrastructure;

internal sealed class CommissionRepository(
    CommissionsDbContext db,
    IUtcTime utcTime) : ICommissionRepository
{
    public Task<Guid?> FindEventIdAsync(Guid operationId, CancellationToken cancellationToken)
    {
        return db.Events
            .Where(x => x.OperationId == operationId)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<EventDetails?> GetDetailsAsync(Guid eventId, CancellationToken cancellationToken)
    {
        var profitEvent = await db.Events
            .Where(x => x.Id == eventId)
            .Select(x => new ProfitEvent(x.OperationId, x.OwnerExternalId, x.Profit, x.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);
        if (profitEvent is null)
        {
            return null;
        }

        var commissions = await db.Commissions
            .Where(x => x.EventId == eventId)
            .OrderBy(x => x.Level)
            .ThenBy(x => x.Id)
            .Select(x => new AccruedCommission(
                x.Id,
                profitEvent.OperationId,
                x.PartnerExternalId,
                x.Level,
                x.Amount,
                x.SchemaType,
                x.PaidAt))
            .ToListAsync(cancellationToken);

        return new EventDetails(profitEvent, commissions);
    }

    public async Task AddEventAsync(
        Event profitEvent,
        IReadOnlyList<Commission> commissions,
        CancellationToken cancellationToken)
    {
        await db.Events.AddAsync(profitEvent, cancellationToken);
        foreach (var commission in commissions)
        {
            commission.EventId = profitEvent.Id;
        }

        if (commissions.Count > 0)
        {
            await db.Commissions.AddRangeAsync(commissions, cancellationToken);
        }
    }

    public async Task<IReadOnlyList<ProfitEvent>> ListByOwnerAsync(
        Guid ownerExternalId,
        CancellationToken cancellationToken)
    {
        return await db.Events
            .Where(x => x.OwnerExternalId == ownerExternalId)
            .OrderByDescending(x => x.CreatedAt)
            .ThenBy(x => x.OperationId)
            .Select(x => new ProfitEvent(x.OperationId, x.OwnerExternalId, x.Profit, x.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public Task<SchemaType?> GetSchemaAsync(CancellationToken cancellationToken)
    {
        return db.SchemaSettings
            .Where(x => x.Enabled)
            .Select(x => (SchemaType?)x.SchemaType)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task SetSchemaAsync(SchemaType schema, CancellationToken cancellationToken)
    {
        var exists = await db.SchemaSettings.AnyAsync(x => x.SchemaType == schema, cancellationToken);
        if (!exists)
        {
            throw new SchemaNotFoundException();
        }

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        await db.SchemaSettings
            .Where(x => x.Enabled)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.Enabled, false), cancellationToken);

        await db.SchemaSettings
            .Where(x => x.SchemaType == schema)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.Enabled, true), cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UnpaidCommission>> GetUnpaidAsync(
        int limit,
        TimeSpan claimTimeout,
        CancellationToken cancellationToken)
    {
        var now = utcTime.Now;
        var availableAfter = now + claimTimeout;

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var claimed = await db.Commissions
            .Where(x => x.PaidAt == null && (x.AvailableAfter == null || x.AvailableAfter < now))
            .OrderBy(x => x.Id)
            .Take(limit)
            .ForUpdate(LockBehavior.SkipLocked)
            .ToListAsync(cancellationToken);

        if (claimed.Count == 0)
        {
            await transaction.CommitAsync(cancellationToken);
            return [];
        }

        foreach (var commission in claimed)
        {
            commission.AvailableAfter = availableAfter;
        }

        await db.SaveChangesAsync(cancellationToken);

        var eventIds = claimed.Select(x => x.EventId).ToHashSet();
        var operationByEvent = await db.Events
            .Where(x => eventIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.OperationId, cancellationToken);

        var unpaid = claimed.ConvertAll(x => new UnpaidCommission(
            x.Id,
            x.PartnerExternalId,
            x.Amount,
            operationByEvent[x.EventId]));

        await transaction.CommitAsync(cancellationToken);
        return unpaid;
    }

    public async Task<IReadOnlyList<Guid>> FindExistingIdsAsync(
        IReadOnlyList<Guid> ids,
        CancellationToken cancellationToken)
    {
        return await db.Commissions
            .Where(x => ids.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);
    }

    public Task AckPaidAsync(IReadOnlyList<Guid> ids, CancellationToken cancellationToken)
    {
        var now = utcTime.Now;
        return db.Commissions
            .Where(x => ids.Contains(x.Id) && x.PaidAt == null)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.PaidAt, now)
                    .SetProperty(x => x.AvailableAfter, (DateTimeOffset?)null),
                cancellationToken);
    }

    public void ClearTracked()
    {
        db.ChangeTracker.Clear();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return db.SaveChangesAsync(cancellationToken);
    }
}
