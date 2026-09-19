using PartnerCommissions.Commissions.Api.Domain;

namespace PartnerCommissions.Commissions.Api.Application;

public interface ICommissionsService
{
    Task<(EventDetails Details, bool Created)> AcceptAsync(
        Guid operationId,
        Guid ownerExternalId,
        decimal profit,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ProfitEvent>> ListByOwnerAsync(Guid ownerExternalId, CancellationToken cancellationToken);

    Task<EventDetails> GetAsync(Guid operationId, CancellationToken cancellationToken);

    Task<SchemaType> GetSchemaAsync(CancellationToken cancellationToken);

    Task<SchemaType> SetSchemaAsync(string schemaType, CancellationToken cancellationToken);

    Task<IReadOnlyList<UnpaidCommission>> GetUnpaidAsync(
        int limit,
        int claimTimeoutSeconds,
        CancellationToken cancellationToken);

    Task AckPaidAsync(IReadOnlyList<Guid> ids, CancellationToken cancellationToken);
}
