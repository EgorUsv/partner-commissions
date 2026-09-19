namespace PartnerCommissions.Wallets.Api.Domain;

public sealed record UnpaidCommission(
    Guid Id,
    Guid PartnerExternalId,
    decimal Amount,
    Guid EventOperationId);

public sealed record PayoutRecord(
    Guid Id,
    Guid CommissionId,
    Guid PartnerExternalId,
    decimal Amount,
    Guid EventOperationId,
    DateTimeOffset CreatedAt);

public sealed record PendingAck(Guid Id, IReadOnlyList<Guid> CommissionIds);

public sealed record CreditResult(int Credited, int AlreadyPaid);

public sealed record PayoutRunResult(int Credited, int AlreadyPaid, int Acked);
