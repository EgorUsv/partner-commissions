using PartnerCommissions.Commissions.Api.Infrastructure.Models;

namespace PartnerCommissions.Commissions.Api.Domain;

public interface ICommissionRepository
{
    Task<Guid?> FindEventIdAsync(Guid operationId, CancellationToken cancellationToken);
    Task<EventDetails?> GetDetailsAsync(Guid eventId, CancellationToken cancellationToken);
    Task AddEventAsync(Event profitEvent, IReadOnlyList<Commission> commissions, CancellationToken cancellationToken);
    Task<IReadOnlyList<ProfitEvent>> ListByOwnerAsync(Guid ownerExternalId, CancellationToken cancellationToken);
    Task<SchemaType?> GetSchemaAsync(CancellationToken cancellationToken);
    Task SetSchemaAsync(SchemaType schema, CancellationToken cancellationToken);
    Task<IReadOnlyList<UnpaidCommission>> GetUnpaidAsync(
        int limit,
        TimeSpan claimTimeout,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<Guid>> FindExistingIdsAsync(IReadOnlyList<Guid> ids, CancellationToken cancellationToken);
    Task AckPaidAsync(IReadOnlyList<Guid> ids, CancellationToken cancellationToken);
    void ClearTracked();
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
