using System.Globalization;
using Grpc.Core;
using Polly.CircuitBreaker;
using Polly.Timeout;
using PartnerCommissions.Contracts.Grpc;
using PartnerCommissions.Wallets.Domain;
using UnpaidCommission = PartnerCommissions.Wallets.Domain.UnpaidCommission;

namespace PartnerCommissions.Wallets.Api.Grpc;

internal sealed class CommissionPayoutsService(CommissionPayouts.CommissionPayoutsClient client) : ICommissionPayouts
{
    public async Task<IReadOnlyList<UnpaidCommission>> GetUnpaidAsync(
        int limit,
        int claimTimeoutSeconds,
        CancellationToken cancellationToken)
    {
        GetUnpaidReply reply;
        try
        {
            reply = await client.GetUnpaidAsync(
                new GetUnpaidRequest
                {
                    Limit = limit,
                    ClaimTimeoutSeconds = claimTimeoutSeconds
                },
                cancellationToken: cancellationToken);
        }
        catch (Exception exception) when (IsUnavailable(exception))
        {
            throw new CommissionsUnavailableException();
        }

        return reply.Commissions.Select(Parse).ToList();
    }

    public async Task AckPaidAsync(IReadOnlyList<Guid> ids, CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            return;
        }

        var request = new AckPaidRequest();
        request.Ids.AddRange(ids.Select(static x => x.ToString("D")));
        try
        {
            await client.AckPaidAsync(request, cancellationToken: cancellationToken);
        }
        catch (Exception exception) when (IsUnavailable(exception))
        {
            throw new CommissionsUnavailableException();
        }
    }

    private static bool IsUnavailable(Exception exception)
    {
        return exception is RpcException { StatusCode: not StatusCode.Cancelled }
            or HttpRequestException
            or BrokenCircuitException
            or TimeoutRejectedException;
    }

    private static UnpaidCommission Parse(Contracts.Grpc.UnpaidCommission commission)
    {
        return new UnpaidCommission(
            RequireGuid(commission.Id),
            RequireGuid(commission.PartnerExternalId),
            RequireAmount(commission.Amount),
            RequireGuid(commission.EventOperationId));
    }

    private static Guid RequireGuid(string value)
    {
        if (Guid.TryParse(value, out var id) && id != Guid.Empty)
        {
            return id;
        }

        throw new CommissionsUnavailableException();
    }

    private static decimal RequireAmount(string value)
    {
        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount)
            && amount > 0)
        {
            return amount;
        }

        throw new CommissionsUnavailableException();
    }
}
