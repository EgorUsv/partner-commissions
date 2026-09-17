namespace PartnerCommissions.Commissions.Domain;

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

public sealed class EventNotFoundException(Guid operationId)
    : DomainException(
        404,
        "event_not_found",
        "Event was not found.",
        ("operationId", operationId.ToString("D")))
{
    public Guid OperationId { get; } = operationId;
}

public sealed class EventOwnerNotFoundException(Guid ownerExternalId)
    : DomainException(
        404,
        "event_owner_not_found",
        "Event owner was not found.",
        ("ownerExternalId", ownerExternalId.ToString("D")))
{
    public Guid OwnerExternalId { get; } = ownerExternalId;
}

public sealed class PartnersUnavailableException()
    : DomainException(
        503,
        "partners_unavailable",
        "Partners are unavailable.")
{
}

public sealed class InvalidRequestException(string message)
    : DomainException(400, "invalid_request", message)
{
}

public sealed class InvalidSchemaException(string schemaType)
    : DomainException(
        400,
        "invalid_schema",
        "Schema type must be Linear or Fibonacci.",
        ("schemaType", schemaType))
{
    public string SchemaType { get; } = schemaType;
}

public sealed class SchemaNotFoundException()
    : DomainException(
        500,
        "schema_not_found",
        "Current schema setting is missing.")
{
}

public sealed class CommissionsNotFoundException(IReadOnlyList<Guid> ids)
    : DomainException(
        404,
        "commission_not_found",
        "One or more commissions were not found.",
        ("ids", string.Join(',', ids.Select(static x => x.ToString("D")))))
{
    public IReadOnlyList<Guid> Ids { get; } = ids;
}
