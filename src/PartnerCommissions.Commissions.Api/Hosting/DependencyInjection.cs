using EFCore.PostgresExtensions.Extensions;
using Microsoft.EntityFrameworkCore;
using PartnerCommissions.Commissions.Api.Application;
using PartnerCommissions.Commissions.Api.Domain;
using PartnerCommissions.Commissions.Api.Infrastructure;
using PartnerCommissions.Contracts.Grpc;

namespace PartnerCommissions.Commissions.Api.Hosting;

internal static class DependencyInjection
{
    public static WebApplicationBuilder AddApi(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString(CommissionsOptions.SectionName)
            ?? throw new InvalidOperationException("Database connection string is missing.");

        builder.Services.AddDbContext<CommissionsDbContext>(options =>
            options.UseNpgsql(connectionString).UseQueryLocks());
        builder.Services.Configure<CommissionsOptions>(builder.Configuration.GetSection(CommissionsOptions.SectionName));
        builder.Services.Configure<UsersGrpcOptions>(builder.Configuration.GetSection(UsersGrpcOptions.SectionName));
        builder.Services.AddSingleton<IUtcTime, UtcTime>();
        builder.Services.AddSingleton<ICommissionCalculator, CommissionCalculator>();
        builder.Services.AddScoped<ICommissionRepository, CommissionRepository>();
        builder.Services.AddScoped<IPartnerLookup, PartnerLookupGateway>();
        builder.Services.AddScoped<ICommissionsService, CommissionsService>();

        var usersAddress = builder.Configuration[$"{UsersGrpcOptions.SectionName}:GrpcAddress"]
            ?? throw new InvalidOperationException("Users gRPC address is missing.");

        builder.Services.AddGrpcClient<UserInviters.UserInvitersClient>(options =>
        {
            options.Address = new Uri(usersAddress);
        }).AddStandardResilienceHandler();

        builder.Services.AddGrpc();
        builder.Services.AddExceptionHandler<DomainExceptionHandler>();
        builder.Services.AddProblemDetails();
        return builder;
    }

    public static async Task MigrateAsync(this WebApplicationBuilder builder)
    {
        await using var app = builder.Build();
        await using var scope = app.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<CommissionsDbContext>().Database.MigrateAsync();
    }
}
