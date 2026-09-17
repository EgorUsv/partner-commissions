using PartnerCommissions.Users.Domain;

namespace PartnerCommissions.Users.Infrastructure;

internal sealed class UtcTime : IUtcTime
{
    public DateTimeOffset Now => DateTimeOffset.UtcNow;
}
