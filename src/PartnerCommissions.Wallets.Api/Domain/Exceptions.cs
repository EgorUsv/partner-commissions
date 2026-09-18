using System.Globalization;

namespace PartnerCommissions.Wallets.Domain;

public abstract class DomainException : Exception
{
    protected DomainException(int statusCode, string errorCode, string message, params (string Key, string Value)[] properties)
        : this(statusCode, errorCode, message, innerException: null, properties)
    {
    }

    protected DomainException(
        int statusCode,
        string errorCode,
        string message,
        Exception? innerException,
        params (string Key, string Value)[] properties)
        : base(message, innerException)
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

public sealed class CommissionsUnavailableException()
    : DomainException(
        503,
        "commissions_unavailable",
        "Commissions are unavailable.")
{
}

public sealed class WalletCreditConflictException(int maxAttempts, Exception? innerException = null)
    : DomainException(
        409,
        "wallet_credit_conflict",
        "Wallet credit failed after concurrent writes.",
        innerException,
        ("maxCreditAttempts", maxAttempts.ToString(CultureInfo.InvariantCulture)))
{
}
