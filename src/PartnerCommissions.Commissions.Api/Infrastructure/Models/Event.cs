namespace PartnerCommissions.Commissions.Api.Infrastructure.Models;

public sealed class Event
{
    public Guid Id { get; private set; }
    public Guid OperationId { get; set; }
    public Guid OwnerExternalId { get; set; }
    public decimal Profit { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
