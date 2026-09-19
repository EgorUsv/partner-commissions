using System.Globalization;
using Grpc.Core;
using PartnerCommissions.Contracts.Grpc;
using PartnerCommissions.Users.Api.Hosting;
using PartnerCommissions.Users.Api.Application;
using PartnerCommissions.Users.Api.Domain;

namespace PartnerCommissions.Users.Api.Grpc;

public sealed class UserInvitersGrpcService(
    IUsersService users,
    ILogger<UserInvitersGrpcService> logger) : UserInviters.UserInvitersBase
{
    public override async Task<GetInvitersReply> GetInviters(
        GetInvitersRequest request,
        ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.ExternalId, out var externalId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "external_id must be a UUID."));
            }

            var inviters = await users.GetInvitersAsync(externalId, context.CancellationToken);
            logger.InvitersReturned(externalId, inviters.Count);
            var reply = new GetInvitersReply();
            reply.Inviters.AddRange(inviters.Select(x => new Contracts.Grpc.Inviter
            {
                ExternalId = x.ExternalId.ToString(),
                Level = x.Level
            }));
            return reply;
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
