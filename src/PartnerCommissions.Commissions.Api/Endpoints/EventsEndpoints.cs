using PartnerCommissions.Commissions.Api.Application;
using PartnerCommissions.Commissions.Api.Domain;

namespace PartnerCommissions.Commissions.Api.Endpoints;

public static class EventsEndpoints
{
    public static IEndpointRouteBuilder MapEventsApi(this IEndpointRouteBuilder app)
    {
        var events = app.MapGroup("/api/events");
        events.MapPost("/", CreateAsync);
        events.MapGet("/", ListAsync);
        events.MapGet("/{operationId}", GetAsync);
        return app;
    }

    private static async Task<IResult> CreateAsync(
        CreateEventRequest? request,
        ICommissionsService commissions,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            throw new InvalidRequestException("Request body is required.");
        }

        var (details, created) = await commissions.AcceptAsync(
            request.OperationId,
            request.OwnerExternalId,
            request.Profit,
            cancellationToken);
        var body = ToDto(details);
        return created
            ? TypedResults.Created($"/api/events/{body.OperationId}", body)
            : TypedResults.Ok(body);
    }

    private static async Task<IResult> ListAsync(
        Guid ownerExternalId,
        ICommissionsService commissions,
        CancellationToken cancellationToken)
    {
        var events = await commissions.ListByOwnerAsync(ownerExternalId, cancellationToken);
        return TypedResults.Ok(events.Select(x => new EventSummaryDto(
            x.OperationId,
            x.OwnerExternalId,
            x.Profit,
            x.CreatedAt)).ToList());
    }

    private static async Task<IResult> GetAsync(
        Guid operationId,
        ICommissionsService commissions,
        CancellationToken cancellationToken)
    {
        var details = await commissions.GetAsync(operationId, cancellationToken);
        return TypedResults.Ok(ToDto(details));
    }

    private static EventDetailsDto ToDto(EventDetails details)
    {
        return new EventDetailsDto(
            details.Event.OperationId,
            details.Event.OwnerExternalId,
            details.Event.Profit,
            details.Event.CreatedAt,
            details.Commissions.Select(x => new CommissionDto(
                x.Id,
                x.PartnerExternalId,
                x.Level,
                x.Amount,
                x.SchemaType.ToString(),
                x.PaidAt)).ToList());
    }
}
