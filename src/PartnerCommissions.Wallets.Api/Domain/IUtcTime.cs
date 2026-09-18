namespace PartnerCommissions.Wallets.Domain;

public interface IUtcTime
{
    DateTimeOffset Now { get; }
}
