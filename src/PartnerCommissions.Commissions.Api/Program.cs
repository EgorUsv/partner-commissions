using Microsoft.EntityFrameworkCore;
using PartnerCommissions.Commissions.Api.Endpoints;
using PartnerCommissions.Commissions.Api.Grpc;
using PartnerCommissions.Commissions.Api.Hosting;
using PartnerCommissions.Commissions.Api.Options;
using PartnerCommissions.Commissions.Domain;
using PartnerCommissions.Commissions.Infrastructure;
using PartnerCommissions.Contracts.Grpc;
using EFCore.PostgresExtensions.Extensions;
using PartnerCommissions.Commissions.Api.Services.Commissions;
using PartnerCommissions.Commissions.Api.Services.Calculator;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<CommissionsOptions>(builder.Configuration.GetSection(CommissionsOptions.SectionName));
builder.Services.Configure<UsersGrpcOptions>(builder.Configuration.GetSection(UsersGrpcOptions.SectionName));

var connectionString = builder.Configuration.GetConnectionString(CommissionsOptions.SectionName)
    ?? throw new InvalidOperationException("Database connection string is missing.");

builder.Services.AddDbContext<CommissionsDbContext>(options =>
    options.UseNpgsql(connectionString).UseQueryLocks());

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

var app = builder.Build();

app.UseExceptionHandler();
app.MapEventsApi();
app.MapSchemaApi();
app.MapGrpcService<CommissionPayoutsGrpcService>();
app.MapHealthApi();
app.Run();
