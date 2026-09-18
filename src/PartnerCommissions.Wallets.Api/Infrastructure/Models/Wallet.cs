namespace PartnerCommissions.Wallets.Infrastructure.Models;

public sealed class Wallet
{
    public Guid PartnerExternalId { get; set; }
    public decimal Balance { get; set; }
}
