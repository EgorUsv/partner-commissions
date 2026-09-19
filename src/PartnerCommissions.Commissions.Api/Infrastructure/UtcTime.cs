using PartnerCommissions.Commissions.Api.Domain;

namespace PartnerCommissions.Commissions.Api.Infrastructure;

internal sealed class UtcTime : IUtcTime
{
    public DateTimeOffset Now => DateTimeOffset.UtcNow;
}
