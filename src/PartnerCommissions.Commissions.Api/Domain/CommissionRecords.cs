namespace PartnerCommissions.Commissions.Api.Domain;

public sealed record ProfitEvent(
    Guid OperationId,
    Guid OwnerExternalId,
    decimal Profit,
    DateTimeOffset CreatedAt);

public sealed record AccruedCommission(
    Guid Id,
    Guid EventOperationId,
    Guid PartnerExternalId,
    int Level,
    decimal Amount,
    SchemaType SchemaType,
    DateTimeOffset? PaidAt);

public sealed record EventDetails(ProfitEvent Event, IReadOnlyList<AccruedCommission> Commissions);

public sealed record UnpaidCommission(Guid Id, Guid PartnerExternalId, decimal Amount, Guid EventOperationId);
