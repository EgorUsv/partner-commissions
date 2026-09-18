namespace PartnerCommissions.Wallets.Api.Options;

public sealed class WalletsOptions
{
    public const string SectionName = "Wallets";

    public int BatchLimit { get; init; } = 100;

    public TimeSpan ClaimTimeout { get; init; } = TimeSpan.FromMinutes(5);

    public TimeSpan PayoutInterval { get; init; } = TimeSpan.FromMinutes(1);

    public TimeSpan AckRetry { get; init; } = TimeSpan.FromSeconds(30);

    public int MaxCreditAttempts { get; init; } = 5;
}

public sealed class CommissionsGrpcOptions
{
    public const string SectionName = "Commissions";

    public required string GrpcAddress { get; init; }
}
