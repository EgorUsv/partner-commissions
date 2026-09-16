namespace PartnerCommissions.Users.Infrastructure.Models;

public sealed class InviterTreeRow
{
    public Guid ExternalId { get; private set; }
    public Guid? InviterId { get; private set; }
    public int Level { get; private set; }
}
