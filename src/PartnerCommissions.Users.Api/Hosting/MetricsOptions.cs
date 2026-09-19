namespace PartnerCommissions.Users.Api.Hosting;

public sealed class MetricsOptions
{
    public const string SectionName = "Metrics";

    public required string MeterName { get; init; }

    public required string UsersCreated { get; init; }
}
