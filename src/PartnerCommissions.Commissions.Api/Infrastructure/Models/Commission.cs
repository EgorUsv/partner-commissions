using PartnerCommissions.Commissions.Api.Domain;

namespace PartnerCommissions.Commissions.Api.Infrastructure.Models;

public sealed class Commission
{
    public Guid Id { get; private set; }
    public Guid EventId { get; set; }
    public Guid PartnerExternalId { get; set; }
    public int Level { get; set; }
    public decimal Amount { get; set; }
    public SchemaType SchemaType { get; set; }
    public DateTimeOffset? PaidAt { get; set; }
    public DateTimeOffset? AvailableAfter { get; set; }
}
