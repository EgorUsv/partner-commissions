using PartnerCommissions.Users.Api.Domain;

namespace PartnerCommissions.Users.Api.Infrastructure;

internal sealed class UtcTime : IUtcTime
{
    public DateTimeOffset Now => DateTimeOffset.UtcNow;
}
