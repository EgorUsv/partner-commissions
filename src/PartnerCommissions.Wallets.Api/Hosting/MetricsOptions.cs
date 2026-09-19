namespace PartnerCommissions.Wallets.Api.Hosting;

public sealed class MetricsOptions
{
    public const string SectionName = "Metrics";

    public required string MeterName { get; init; }

    public required string CommissionsCredited { get; init; }

    public required string CommissionsAlreadyPaid { get; init; }

    public required string CommissionsAcked { get; init; }
}
