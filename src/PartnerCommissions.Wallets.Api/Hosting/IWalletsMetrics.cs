namespace PartnerCommissions.Wallets.Api.Hosting;

public interface IWalletsMetrics
{
    void PayoutRun(int creditedCount, int alreadyPaidCount, int ackedCount);
}
