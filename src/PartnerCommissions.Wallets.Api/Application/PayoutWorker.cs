using Microsoft.Extensions.Options;
using PartnerCommissions.Wallets.Api.Hosting;

namespace PartnerCommissions.Wallets.Api.Application;

internal sealed class PayoutWorker(
    IServiceScopeFactory scopes,
    IOptions<WalletsOptions> options,
    ILogger<PayoutWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopes.CreateAsyncScope();
                var wallets = scope.ServiceProvider.GetRequiredService<IWalletsService>();
                await wallets.RunPayoutsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.PayoutWorkerFailed(exception);
            }

            await Task.Delay(options.Value.PayoutInterval, stoppingToken);
        }
    }
}
