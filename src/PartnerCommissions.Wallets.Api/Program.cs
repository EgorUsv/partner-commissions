using EFCore.PostgresExtensions.Extensions;
using Microsoft.EntityFrameworkCore;
using PartnerCommissions.Contracts.Grpc;
using PartnerCommissions.Wallets.Api.Endpoints;
using PartnerCommissions.Wallets.Api.Grpc;
using PartnerCommissions.Wallets.Api.Hosting;
using PartnerCommissions.Wallets.Api.Options;
using PartnerCommissions.Wallets.Api.Services.Wallets;
using PartnerCommissions.Wallets.Api.Workers;
using PartnerCommissions.Wallets.Domain;
using PartnerCommissions.Wallets.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString(WalletsOptions.SectionName)
    ?? throw new InvalidOperationException("Database connection string is missing.");
builder.Services.AddDbContext<WalletsDbContext>(options =>
    options.UseNpgsql(connectionString).UseQueryLocks());

if (args is ["migrate"])
{
    await using var migrator = builder.Build();
    await using var scope = migrator.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<WalletsDbContext>().Database.MigrateAsync();
    return;
}

builder.Services.Configure<WalletsOptions>(builder.Configuration.GetSection(WalletsOptions.SectionName));
builder.Services.Configure<CommissionsGrpcOptions>(builder.Configuration.GetSection(CommissionsGrpcOptions.SectionName));
builder.Services.AddSingleton<IUtcTime, UtcTime>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<ICommissionPayouts, CommissionPayoutsService>();
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

var app = builder.Build();

app.UseExceptionHandler();
app.MapWalletsApi();
app.MapHealthApi();
app.Run();
