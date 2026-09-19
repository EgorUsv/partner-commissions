using System.Diagnostics.Metrics;
using Microsoft.Extensions.Options;

namespace PartnerCommissions.Wallets.Api.Hosting;

public sealed class WalletsMetrics : IWalletsMetrics
{
    private readonly Counter<long> credited;
    private readonly Counter<long> alreadyPaid;
    private readonly Counter<long> acked;

    public WalletsMetrics(IMeterFactory meters, IOptions<MetricsOptions> options)
    {
        var settings = options.Value;
        var meter = meters.Create(settings.MeterName);
        credited = meter.CreateCounter<long>(settings.CommissionsCredited);
        alreadyPaid = meter.CreateCounter<long>(settings.CommissionsAlreadyPaid);
        acked = meter.CreateCounter<long>(settings.CommissionsAcked);
    }

    public void PayoutRun(int creditedCount, int alreadyPaidCount, int ackedCount)
    {
        credited.Add(creditedCount);
        alreadyPaid.Add(alreadyPaidCount);
        acked.Add(ackedCount);
    }
}
