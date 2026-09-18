namespace PartnerCommissions.Wallets.Infrastructure.Models;

public sealed class OutboxMessage
{
    public Guid Id { get; private set; }
    public Guid[] CommissionIds { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
    public DateTimeOffset? AvailableAfter { get; set; }
}
