using System.Globalization;
using Grpc.Core;
using PartnerCommissions.Commissions.Api.Logging;
using PartnerCommissions.Commissions.Api.Services.Commissions;
using PartnerCommissions.Commissions.Domain;
using PartnerCommissions.Contracts.Grpc;

namespace PartnerCommissions.Commissions.Api.Grpc;

public sealed class CommissionPayoutsGrpcService(
    ICommissionsService commissions,
    ILogger<CommissionPayoutsGrpcService> logger) : CommissionPayouts.CommissionPayoutsBase
{
    public override async Task<GetUnpaidReply> GetUnpaid(
        GetUnpaidRequest request,
        ServerCallContext context)
    {
        try
        {
            var unpaid = await commissions.GetUnpaidAsync(
                request.Limit,
                request.ClaimTimeoutSeconds,
                context.CancellationToken);
            logger.UnpaidReturned(unpaid.Count);
            var reply = new GetUnpaidReply();
            reply.Commissions.AddRange(unpaid.Select(x => new Contracts.Grpc.UnpaidCommission
            {
                Id = x.Id.ToString("D"),
                PartnerExternalId = x.PartnerExternalId.ToString("D"),
                Amount = x.Amount.ToString(CultureInfo.InvariantCulture),
                EventOperationId = x.EventOperationId.ToString("D")
            }));
            return reply;
        }
        catch (DomainException exception)
        {
            logger.DomainError(exception, exception.ErrorCode, exception.StatusCode, exception.FormatDetails());
            throw MapRpc(exception);
        }
    }

    public override async Task<AckPaidReply> AckPaid(
        AckPaidRequest request,
        ServerCallContext context)
    {
        try
        {
            var ids = new List<Guid>(request.Ids.Count);
            foreach (var value in request.Ids)
            {
                if (!Guid.TryParse(value, out var id))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "ids must be UUIDs."));
                }

                ids.Add(id);
            }

            await commissions.AckPaidAsync(ids, context.CancellationToken);
            return new AckPaidReply();
        }
        catch (DomainException exception)
        {
            logger.DomainError(exception, exception.ErrorCode, exception.StatusCode, exception.FormatDetails());
            throw MapRpc(exception);
        }
    }

    private static RpcException MapRpc(DomainException exception)
    {
        var statusCode = exception.StatusCode switch
        {
            404 => StatusCode.NotFound,
            409 => StatusCode.AlreadyExists,
            503 => StatusCode.Unavailable,
            _ => StatusCode.InvalidArgument
        };

        var trailers = new Metadata
        {
            { "error-code", exception.ErrorCode },
            { "status-code", exception.StatusCode.ToString(CultureInfo.InvariantCulture) }
        };
        foreach (var (key, value) in exception.Properties)
        {
            trailers.Add(key.ToLowerInvariant(), value);
        }

        return new RpcException(new Status(statusCode, exception.Message), trailers);
    }
}
