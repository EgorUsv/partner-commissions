namespace PartnerCommissions.Users.Api.Infrastructure.Models;

public sealed class User
{
    public Guid ExternalId { get; private set; }
    public Guid? InviterId { get; set; }
}
