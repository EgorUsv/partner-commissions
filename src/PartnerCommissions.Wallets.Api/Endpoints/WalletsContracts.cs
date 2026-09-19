namespace PartnerCommissions.Wallets.Api.Endpoints;

public sealed record WalletResponse(Guid PartnerExternalId, decimal Balance);

public sealed record PayoutDto(
    Guid Id,
    Guid CommissionId,
    Guid EventOperationId,
    decimal Amount,
    DateTimeOffset CreatedAt);

public sealed record PayoutsResponse(Guid PartnerExternalId, IReadOnlyList<PayoutDto> Payouts);
