namespace PartnerCommissions.Commissions.Domain;

public interface IPartnerLookup
{
    Task<IReadOnlyList<Partner>> GetByOwnerAsync(Guid ownerExternalId, CancellationToken cancellationToken);
}
