using PartnerCommissions.Commissions.Domain;

namespace PartnerCommissions.Commissions.Infrastructure;

internal sealed class UtcTime : IUtcTime
{
    public DateTimeOffset Now => DateTimeOffset.UtcNow;
}
