using Grpc.Core;
using Polly.CircuitBreaker;
using Polly.Timeout;
using PartnerCommissions.Commissions.Api.Domain;
using PartnerCommissions.Contracts.Grpc;

namespace PartnerCommissions.Commissions.Api.Infrastructure;

internal sealed class PartnerLookupGateway(UserInviters.UserInvitersClient client) : IPartnerLookup
{
    public async Task<IReadOnlyList<Partner>> GetByOwnerAsync(Guid ownerExternalId, CancellationToken cancellationToken)
    {
        GetInvitersReply reply;
        try
        {
            reply = await client.GetInvitersAsync(
                new GetInvitersRequest { ExternalId = ownerExternalId.ToString("D") },
                cancellationToken: cancellationToken);
        }
        catch (RpcException exception) when (exception.StatusCode == StatusCode.NotFound)
        {
            throw new EventOwnerNotFoundException(ownerExternalId);
        }
        catch (RpcException exception) when (exception.StatusCode == StatusCode.Cancelled)
        {
            throw;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (
            exception is RpcException
                or HttpRequestException
                or BrokenCircuitException
                or TimeoutRejectedException)
        {
            throw new PartnersUnavailableException();
        }

        var partners = new List<Partner>(reply.Inviters.Count);
        foreach (var inviter in reply.Inviters)
        {
            if (!Guid.TryParse(inviter.ExternalId, out var externalId) || inviter.Level < 1)
            {
                throw new PartnersUnavailableException();
            }

            partners.Add(new Partner(externalId, inviter.Level));
        }

        return partners;
    }
}
