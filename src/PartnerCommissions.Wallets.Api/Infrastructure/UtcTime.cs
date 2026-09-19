using PartnerCommissions.Wallets.Api.Domain;

namespace PartnerCommissions.Wallets.Api.Infrastructure;

internal sealed class UtcTime : IUtcTime
{
    public DateTimeOffset Now => DateTimeOffset.UtcNow;
}
