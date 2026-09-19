using PartnerCommissions.Commissions.Api.Application;
using PartnerCommissions.Commissions.Api.Domain;

namespace PartnerCommissions.Commissions.Api.Endpoints;

public static class SchemaEndpoints
{
    public static IEndpointRouteBuilder MapSchemaApi(this IEndpointRouteBuilder app)
    {
        var schema = app.MapGroup("/api/schema");
        schema.MapGet("/", GetAsync);
        schema.MapPut("/", SetAsync);
        return app;
    }

    private static async Task<IResult> GetAsync(
        ICommissionsService commissions,
        CancellationToken cancellationToken)
    {
        var schema = await commissions.GetSchemaAsync(cancellationToken);
        return TypedResults.Ok(new SchemaResponse(schema.ToString()));
    }

    private static async Task<IResult> SetAsync(
        SetSchemaRequest? request,
        ICommissionsService commissions,
        CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.SchemaType))
        {
            throw new InvalidRequestException("Request body is required.");
        }

        var schema = await commissions.SetSchemaAsync(request.SchemaType, cancellationToken);
        return TypedResults.Ok(new SchemaResponse(schema.ToString()));
    }
}
