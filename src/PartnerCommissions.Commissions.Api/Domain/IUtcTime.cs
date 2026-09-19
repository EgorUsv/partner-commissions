namespace PartnerCommissions.Commissions.Api.Domain;

public interface IUtcTime
{
    DateTimeOffset Now { get; }
}
