using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using PartnerCommissions.Users.Api.Application;
using PartnerCommissions.Users.Api.Domain;
using PartnerCommissions.Users.Api.Infrastructure;
using System.Text.Json.Serialization;

namespace PartnerCommissions.Users.Api.Hosting;

internal static class DependencyInjection
{
    public static WebApplicationBuilder AddApi(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString(UsersOptions.SectionName)
            ?? throw new InvalidOperationException("Database connection string is missing.");

        builder.Services.AddDbContext<UsersDbContext>(options => options.UseNpgsql(connectionString));
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
        builder.Services.Configure<UsersOptions>(builder.Configuration.GetSection(UsersOptions.SectionName));
        builder.Services.Configure<MetricsOptions>(builder.Configuration.GetSection(MetricsOptions.SectionName));
        builder.Services.AddSingleton<IUtcTime, UtcTime>();
        builder.Services.AddSingleton<IUsersMetrics, UsersMetrics>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IUsersService, UsersService>();
        builder.Services.AddGrpc();
        builder.Services.AddExceptionHandler<DomainExceptionHandler>();
        builder.Services.AddProblemDetails();
        builder.Services.AddPrometheusMetrics(RequireMeterName(builder.Configuration));
        return builder;
    }

    public static async Task MigrateAsync(this WebApplicationBuilder builder)
    {
        await using var app = builder.Build();
        await using var scope = app.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<UsersDbContext>().Database.MigrateAsync();
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
