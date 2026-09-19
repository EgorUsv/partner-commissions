namespace PartnerCommissions.Commissions.Api.Hosting;

public sealed class CommissionsOptions
{
    public const string SectionName = "Commissions";

    public int MaxUnpaidLimit { get; init; } = 100;

    public TimeSpan MaxClaimTimeout { get; init; } = TimeSpan.FromMinutes(8);
}

public sealed class UsersGrpcOptions
{
    public const string SectionName = "Users";

    public required string GrpcAddress { get; init; }
}
