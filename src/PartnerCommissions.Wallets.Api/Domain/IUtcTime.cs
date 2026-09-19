namespace PartnerCommissions.Wallets.Api.Domain;

public interface IUtcTime
{
    DateTimeOffset Now { get; }
}
