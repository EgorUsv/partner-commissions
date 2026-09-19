namespace PartnerCommissions.Commissions.Api.Hosting;

public sealed class MetricsOptions
{
    public const string SectionName = "Metrics";

    public required string MeterName { get; init; }

    public required string EventsAccepted { get; init; }

    public required string CommissionsAccrued { get; init; }
}
