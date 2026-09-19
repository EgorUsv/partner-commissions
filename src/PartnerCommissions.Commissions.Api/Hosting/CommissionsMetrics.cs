using System.Diagnostics.Metrics;
using Microsoft.Extensions.Options;

namespace PartnerCommissions.Commissions.Api.Hosting;

public sealed class CommissionsMetrics : ICommissionsMetrics
{
    private readonly Counter<long> eventsAccepted;
    private readonly Counter<long> commissionsAccrued;

    public CommissionsMetrics(IMeterFactory meters, IOptions<MetricsOptions> options)
    {
        var settings = options.Value;
        var meter = meters.Create(settings.MeterName);
        eventsAccepted = meter.CreateCounter<long>(settings.EventsAccepted);
        commissionsAccrued = meter.CreateCounter<long>(settings.CommissionsAccrued);
    }

    public void EventAccepted(int accrued)
    {
        eventsAccepted.Add(1);
        commissionsAccrued.Add(accrued);
    }
}
