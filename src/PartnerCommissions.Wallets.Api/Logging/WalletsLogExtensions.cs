namespace PartnerCommissions.Wallets.Api.Logging;

internal static partial class WalletsLogExtensions
{
    [LoggerMessage(LogLevel.Information, "Payout run completed {Credited} {AlreadyPaid} {Acked}")]
    public static partial void PayoutRunCompleted(
        this ILogger logger,
        int credited,
        int alreadyPaid,
        int acked);

    [LoggerMessage(LogLevel.Information, "AckPaid sent {Messages} {Commissions}")]
    public static partial void PaidAcked(this ILogger logger, int messages, int commissions);

    [LoggerMessage(LogLevel.Debug, "Credit unique conflict, retry {Attempt}/{MaxAttempts}")]
    public static partial void CreditConflict(this ILogger logger, int attempt, int maxAttempts);

    [LoggerMessage(LogLevel.Warning, "AckPaid failed {OutboxId} {CommissionCount}")]
    public static partial void AckFailed(
        this ILogger logger,
        Exception exception,
        Guid outboxId,
        int commissionCount);

    [LoggerMessage(LogLevel.Warning, "Payout worker failed")]
    public static partial void PayoutWorkerFailed(this ILogger logger, Exception exception);

    [LoggerMessage(LogLevel.Warning, "Domain error {ErrorCode} {StatusCode} {Details}")]
    public static partial void DomainError(
        this ILogger logger,
        Exception exception,
        string errorCode,
        int statusCode,
        string details);
}
