namespace PartnerCommissions.Commissions.Api.Hosting;

internal static partial class CommissionsLogExtensions
{
    [LoggerMessage(LogLevel.Information, "Event accepted {OperationId} {OwnerExternalId} {Profit} {CommissionCount}")]
    public static partial void EventAccepted(
        this ILogger logger,
        Guid operationId,
        Guid ownerExternalId,
        decimal profit,
        int commissionCount);

    [LoggerMessage(LogLevel.Information, "Schema changed {SchemaType}")]
    public static partial void SchemaChanged(this ILogger logger, string schemaType);

    [LoggerMessage(LogLevel.Debug, "GetUnpaid completed {Count}")]
    public static partial void UnpaidReturned(this ILogger logger, int count);

    [LoggerMessage(LogLevel.Information, "AckPaid received {Count}")]
    public static partial void PaidAcked(this ILogger logger, int count);

    [LoggerMessage(LogLevel.Warning, "Domain error {ErrorCode} {StatusCode} {Details}")]
    public static partial void DomainError(
        this ILogger logger,
        Exception exception,
        string errorCode,
        int statusCode,
        string details);
}
