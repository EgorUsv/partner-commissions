namespace PartnerCommissions.Commissions.Domain;

public interface IUtcTime
{
    DateTimeOffset Now { get; }
}
