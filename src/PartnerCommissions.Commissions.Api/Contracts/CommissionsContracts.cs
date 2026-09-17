namespace PartnerCommissions.Contracts.Commissions;

public sealed record CreateEventRequest(Guid OperationId, Guid OwnerExternalId, decimal Profit);

public sealed record EventSummaryDto(Guid OperationId, Guid OwnerExternalId, decimal Profit, DateTimeOffset CreatedAt);

public sealed record CommissionDto(
    Guid Id,
    Guid PartnerExternalId,
    int Level,
    decimal Amount,
    string SchemaType,
    DateTimeOffset? PaidAt);

public sealed record EventDetailsDto(
    Guid OperationId,
    Guid OwnerExternalId,
    decimal Profit,
    DateTimeOffset CreatedAt,
    IReadOnlyList<CommissionDto> Commissions);

public sealed record SchemaResponse(string SchemaType);

public sealed record SetSchemaRequest(string SchemaType);
