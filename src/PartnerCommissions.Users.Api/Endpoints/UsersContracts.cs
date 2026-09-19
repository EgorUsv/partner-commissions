using PartnerCommissions.Users.Api.Domain;

namespace PartnerCommissions.Users.Api.Endpoints;

public sealed record CreateUserRequest(Guid? InviterId);

public sealed record UserCreatedResponse(Guid ExternalId);

public sealed record InviterTreeNodeDto(Guid ExternalId, Guid? InviterId, int Level);

public sealed record InviterTreeResponse(Guid ExternalId, TreeDirection Direction, IReadOnlyList<InviterTreeNodeDto> Nodes);
