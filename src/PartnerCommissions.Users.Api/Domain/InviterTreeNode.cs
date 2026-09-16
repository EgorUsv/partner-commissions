namespace PartnerCommissions.Users.Domain;

public sealed record InviterTreeNode(Guid ExternalId, Guid? InviterId, int Level);
