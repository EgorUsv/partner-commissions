namespace PartnerCommissions.Users.Domain;

public interface IUtcTime
{
    DateTimeOffset Now { get; }
}
