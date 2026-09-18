namespace PartnerCommissions.Wallets.Infrastructure.Models;

public sealed class Payout
{
    public Guid Id { get; private set; }
    public Guid CommissionId { get; set; }
    public Guid PartnerExternalId { get; set; }
    public decimal Amount { get; set; }
    public Guid EventOperationId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
