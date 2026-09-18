using PartnerCommissions.Commissions.Infrastructure;

namespace PartnerCommissions.Commissions.Api.Endpoints;

public static class HealthEndpoints
{
    public sealed record Report(bool Live, bool Ready);

    public static IEndpointRouteBuilder MapHealthApi(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", CheckAsync);
        return app;
    }

    private static async Task<IResult> CheckAsync(CommissionsDbContext db, CancellationToken cancellationToken)
    {
        var ready = await db.Database.CanConnectAsync(cancellationToken);
        var report = new Report(Live: true, Ready: ready);
        return TypedResults.Json(
            report,
            statusCode: ready ? StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable);
    }
}
