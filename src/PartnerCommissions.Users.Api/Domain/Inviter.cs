namespace PartnerCommissions.Users.Domain;

public sealed record Inviter(Guid ExternalId, int Level);
