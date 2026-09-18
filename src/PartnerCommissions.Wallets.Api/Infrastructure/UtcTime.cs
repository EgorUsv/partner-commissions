using PartnerCommissions.Wallets.Domain;

namespace PartnerCommissions.Wallets.Infrastructure;

internal sealed class UtcTime : IUtcTime
{
    public DateTimeOffset Now => DateTimeOffset.UtcNow;
}
