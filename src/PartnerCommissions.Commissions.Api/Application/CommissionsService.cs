using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using PartnerCommissions.Commissions.Api.Hosting;
using PartnerCommissions.Commissions.Api.Domain;
using PartnerCommissions.Commissions.Api.Infrastructure.Models;

namespace PartnerCommissions.Commissions.Api.Application;

public sealed class CommissionsService(
    ICommissionRepository commissions,
    IPartnerLookup partnerLookup,
    IUtcTime utcTime,
    ICommissionCalculator calculator,
    IOptions<CommissionsOptions> options,
    ILogger<CommissionsService> logger,
    ICommissionsMetrics metrics) : ICommissionsService
{
    public async Task<(EventDetails Details, bool Created)> AcceptAsync(
        Guid operationId,
        Guid ownerExternalId,
        decimal profit,
        CancellationToken cancellationToken)
    {
        if (operationId == Guid.Empty || ownerExternalId == Guid.Empty)
        {
            throw new InvalidRequestException("operationId and ownerExternalId must be UUIDs.");
        }

        var existingId = await commissions.FindEventIdAsync(operationId, cancellationToken);
        if (existingId is not null)
        {
            return (await LoadDetailsAsync(existingId.Value, operationId, cancellationToken), Created: false);
        }

        var partners = await partnerLookup.GetByOwnerAsync(ownerExternalId, cancellationToken);
        var schema = await RequireSchemaAsync(cancellationToken);
        List<Commission> accrued = [];
        if (profit > 0)
        {
            accrued = partners.Select(partner => new Commission
            {
                PartnerExternalId = partner.ExternalId,
                Level = partner.Level,
                Amount = calculator.Amount(schema, partner.Level, profit),
                SchemaType = schema
            }).ToList();
        }

        var profitEvent = new Event
        {
            OperationId = operationId,
            OwnerExternalId = ownerExternalId,
            Profit = profit,
            CreatedAt = utcTime.Now
        };

        try
        {
            await commissions.AddEventAsync(profitEvent, accrued, cancellationToken);
            await commissions.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            commissions.ClearTracked();
            var replayedId = await commissions.FindEventIdAsync(operationId, cancellationToken);
            if (replayedId is null)
            {
                throw;
            }

            return (await LoadDetailsAsync(replayedId.Value, operationId, cancellationToken), Created: false);
        }

        logger.EventAccepted(operationId, ownerExternalId, profit, accrued.Count);
        metrics.EventAccepted(accrued.Count);
        return (ToDetails(profitEvent, accrued), Created: true);
    }

    public Task<IReadOnlyList<ProfitEvent>> ListByOwnerAsync(
        Guid ownerExternalId,
        CancellationToken cancellationToken)
    {
        if (ownerExternalId == Guid.Empty)
        {
            throw new InvalidRequestException("ownerExternalId must be a UUID.");
        }

        return commissions.ListByOwnerAsync(ownerExternalId, cancellationToken);
    }

    public async Task<EventDetails> GetAsync(Guid operationId, CancellationToken cancellationToken)
    {
        var eventId = await commissions.FindEventIdAsync(operationId, cancellationToken)
            ?? throw new EventNotFoundException(operationId);
        return await LoadDetailsAsync(eventId, operationId, cancellationToken);
    }

    public async Task<SchemaType> GetSchemaAsync(CancellationToken cancellationToken)
    {
        return await RequireSchemaAsync(cancellationToken);
    }

    public async Task<SchemaType> SetSchemaAsync(string schemaType, CancellationToken cancellationToken)
    {
        if (!TryParseSchema(schemaType, out var schema))
        {
            throw new InvalidSchemaException(schemaType);
        }

        await commissions.SetSchemaAsync(schema, cancellationToken);
        logger.SchemaChanged(schema.ToString());
        return schema;
    }

    public async Task<IReadOnlyList<UnpaidCommission>> GetUnpaidAsync(
        int limit,
        int claimTimeoutSeconds,
        CancellationToken cancellationToken)
    {
        if (limit <= 0)
        {
            throw new InvalidRequestException("limit must be greater than 0.");
        }

        if (claimTimeoutSeconds <= 0)
        {
            throw new InvalidRequestException("claimTimeoutSeconds must be greater than 0.");
        }

        var settings = options.Value;
        var take = Math.Min(limit, settings.MaxUnpaidLimit);
        var requested = TimeSpan.FromSeconds(claimTimeoutSeconds);
        var claimTimeout = requested <= settings.MaxClaimTimeout ? requested : settings.MaxClaimTimeout;
        return await commissions.GetUnpaidAsync(take, claimTimeout, cancellationToken);
    }

    public async Task AckPaidAsync(IReadOnlyList<Guid> ids, CancellationToken cancellationToken)
    {
        var unique = ids.Distinct().ToList();
        if (unique.Count == 0)
        {
            return;
        }

        var existing = await commissions.FindExistingIdsAsync(unique, cancellationToken);
        if (existing.Count != unique.Count)
        {
            throw new CommissionsNotFoundException(unique.Except(existing).ToList());
        }

        await commissions.AckPaidAsync(unique, cancellationToken);
        logger.PaidAcked(unique.Count);
    }

    private async Task<EventDetails> LoadDetailsAsync(
        Guid eventId,
        Guid operationId,
        CancellationToken cancellationToken)
    {
        return await commissions.GetDetailsAsync(eventId, cancellationToken)
            ?? throw new EventNotFoundException(operationId);
    }

    private async Task<SchemaType> RequireSchemaAsync(CancellationToken cancellationToken)
    {
        return await commissions.GetSchemaAsync(cancellationToken)
            ?? throw new SchemaNotFoundException();
    }

    private static EventDetails ToDetails(Event profitEvent, IReadOnlyList<Commission> rows)
    {
        return new EventDetails(
            new ProfitEvent(
                profitEvent.OperationId,
                profitEvent.OwnerExternalId,
                profitEvent.Profit.ToMoney(),
                profitEvent.CreatedAt),
            rows.Select(x => new AccruedCommission(
                x.Id,
                profitEvent.OperationId,
                x.PartnerExternalId,
                x.Level,
                x.Amount.ToMoney(),
                x.SchemaType,
                x.PaidAt)).ToList());
    }

    private static bool TryParseSchema(string schemaType, out SchemaType schema)
    {
        return Enum.TryParse(schemaType, ignoreCase: true, out schema)
            && string.Equals(Enum.GetName(schema), schemaType, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsUniqueViolation(DbUpdateException exception)
    {
        return exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
    }
}
