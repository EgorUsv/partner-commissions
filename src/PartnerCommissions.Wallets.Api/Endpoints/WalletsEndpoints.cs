using PartnerCommissions.Wallets.Api.Application;

namespace PartnerCommissions.Wallets.Api.Endpoints;

public static class WalletsEndpoints
{
    public static IEndpointRouteBuilder MapWalletsApi(this IEndpointRouteBuilder app)
    {
        var wallets = app.MapGroup("/api/wallets");
        wallets.MapGet("/{partnerExternalId}", GetAsync);
        wallets.MapGet("/{partnerExternalId}/payouts", ListPayoutsAsync);
        return app;
    }

    private static async Task<IResult> GetAsync(
        Guid partnerExternalId,
        IWalletsService wallets,
        CancellationToken cancellationToken)
    {
        var balance = await wallets.GetBalanceAsync(partnerExternalId, cancellationToken);
        return TypedResults.Ok(new WalletResponse(partnerExternalId, balance));
    }

    private static async Task<IResult> ListPayoutsAsync(
        Guid partnerExternalId,
        IWalletsService wallets,
        CancellationToken cancellationToken)
    {
        var payouts = await wallets.ListPayoutsAsync(partnerExternalId, cancellationToken);
        return TypedResults.Ok(new PayoutsResponse(
            partnerExternalId,
            payouts.Select(x => new PayoutDto(
                x.Id,
                x.CommissionId,
                x.EventOperationId,
                x.Amount,
                x.CreatedAt)).ToList()));
    }
}
