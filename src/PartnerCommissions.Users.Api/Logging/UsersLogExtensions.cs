namespace PartnerCommissions.Users.Api.Logging;

internal static partial class UsersLogExtensions
{
    [LoggerMessage(LogLevel.Information, "User created {ExternalId} {InviterId}")]
    public static partial void UserCreated(this ILogger logger, Guid externalId, Guid? inviterId);

    [LoggerMessage(LogLevel.Warning, "Domain error {ErrorCode} {StatusCode} {Details}")]
    public static partial void DomainError(
        this ILogger logger,
        Exception exception,
        string errorCode,
        int statusCode,
        string details);

    [LoggerMessage(LogLevel.Debug, "GetInviters completed {ExternalId} {InviterCount}")]
    public static partial void InvitersReturned(this ILogger logger, Guid externalId, int inviterCount);
}
