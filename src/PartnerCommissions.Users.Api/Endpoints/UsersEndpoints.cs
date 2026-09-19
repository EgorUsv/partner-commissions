using PartnerCommissions.Users.Api.Application;
using PartnerCommissions.Users.Api.Domain;

namespace PartnerCommissions.Users.Api.Endpoints;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersApi(this IEndpointRouteBuilder app)
    {
        var users = app.MapGroup("/api/users");
        users.MapPost("/", CreateAsync);
        users.MapGet("/{externalId}/tree", GetTreeAsync);
        return app;
    }

    private static async Task<IResult> CreateAsync(
        CreateUserRequest? request,
        IUsersService users,
        CancellationToken cancellationToken)
    {
        var externalId = await users.CreateAsync(request?.InviterId, cancellationToken);
        return TypedResults.Created($"/api/users/{externalId}", new UserCreatedResponse(externalId));
    }

    private static async Task<IResult> GetTreeAsync(
        Guid externalId,
        TreeDirection direction,
        IUsersService users,
        CancellationToken cancellationToken)
    {
        var nodes = await users.GetTreeAsync(externalId, direction, cancellationToken);
        return TypedResults.Ok(new InviterTreeResponse(
            externalId,
            direction,
            nodes.Select(x => new InviterTreeNodeDto(x.ExternalId, x.InviterId, x.Level)).ToList()));
    }
}
