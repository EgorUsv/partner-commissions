using EFCore.PostgresExtensions.Extensions;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using PartnerCommissions.Contracts.Grpc;
using PartnerCommissions.Wallets.Api.Application;
using PartnerCommissions.Wallets.Api.Domain;
using PartnerCommissions.Wallets.Api.Infrastructure;

namespace PartnerCommissions.Wallets.Api.Hosting;

internal static class DependencyInjection
{
    public static WebApplicationBuilder AddApi(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString(WalletsOptions.SectionName)
            ?? throw new InvalidOperationException("Database connection string is missing.");

        builder.Services.AddDbContext<WalletsDbContext>(options =>
            options.UseNpgsql(connectionString).UseQueryLocks());
        builder.Services.Configure<WalletsOptions>(builder.Configuration.GetSection(WalletsOptions.SectionName));
        builder.Services.Configure<CommissionsGrpcOptions>(builder.Configuration.GetSection(CommissionsGrpcOptions.SectionName));
        builder.Services.Configure<MetricsOptions>(builder.Configuration.GetSection(MetricsOptions.SectionName));
        builder.Services.AddSingleton<IUtcTime, UtcTime>();
        builder.Services.AddSingleton<IWalletsMetrics, WalletsMetrics>();
        builder.Services.AddScoped<IWalletRepository, WalletRepository>();
        builder.Services.AddScoped<ICommissionPayouts, CommissionPayoutsGateway>();
        builder.Services.AddScoped<IWalletsService, WalletsService>();
        builder.Services.AddHostedService<PayoutWorker>();

        var commissionsAddress = builder.Configuration[$"{CommissionsGrpcOptions.SectionName}:GrpcAddress"]
            ?? throw new InvalidOperationException("Commissions gRPC address is missing.");

        builder.Services.AddGrpcClient<CommissionPayouts.CommissionPayoutsClient>(options =>
        {
            options.Address = new Uri(commissionsAddress);
        }).AddStandardResilienceHandler();

        builder.Services.AddExceptionHandler<DomainExceptionHandler>();
        builder.Services.AddProblemDetails();
        builder.Services.AddPrometheusMetrics(RequireMeterName(builder.Configuration));
        return builder;
    }

    public static async Task MigrateAsync(this WebApplicationBuilder builder)
    {
        await using var app = builder.Build();
        await using var scope = app.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<WalletsDbContext>().Database.MigrateAsync();
    }

    private static string RequireMeterName(IConfiguration configuration)
    {
        var key = $"{MetricsOptions.SectionName}:{nameof(MetricsOptions.MeterName)}";
        var meterName = configuration[key];
        if (string.IsNullOrWhiteSpace(meterName))
        {
            throw new InvalidOperationException($"{key} is missing.");
        }

        return meterName;
    }

    private static void AddPrometheusMetrics(this IServiceCollection services, string meterName)
    {
        services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics
                    .AddMeter(meterName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddPrometheusExporter();
            });
    }
}
