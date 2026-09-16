using System.Globalization;

namespace PartnerCommissions.Users.Domain;

public abstract class DomainException : Exception
{
    protected DomainException(int statusCode, string errorCode, string message, params (string Key, string Value)[] properties)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        Properties = properties;
    }

    public int StatusCode { get; }

    public string ErrorCode { get; }

    public IReadOnlyList<(string Key, string Value)> Properties { get; }

    public string FormatDetails()
    {
        if (Properties.Count == 0)
        {
            return string.Empty;
        }

        return string.Join(' ', Properties.Select(static x => $"{x.Key}={x.Value}"));
    }
}

public sealed class UserNotFoundException(Guid externalId)
    : DomainException(
        404,
        "user_not_found",
        "User was not found.",
        ("externalId", externalId.ToString("D")))
{
    public Guid ExternalId { get; } = externalId;
}

public sealed class TreeDepthExceededException(int maxTreeDepth, Guid externalId)
    : DomainException(
        400,
        "max_tree_depth",
        "Inviter chain exceeds MaxTreeDepth.",
        ("maxTreeDepth", maxTreeDepth.ToString(CultureInfo.InvariantCulture)),
        ("externalId", externalId.ToString("D")))
{
    public int MaxTreeDepth { get; } = maxTreeDepth;
    public Guid ExternalId { get; } = externalId;
}
