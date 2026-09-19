namespace PartnerCommissions.Users.Api.Domain;

public interface IUtcTime
{
    DateTimeOffset Now { get; }
}
