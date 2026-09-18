using Microsoft.EntityFrameworkCore;
using PartnerCommissions.Users.Api.Endpoints;
using PartnerCommissions.Users.Api.Grpc;
using PartnerCommissions.Users.Api.Hosting;
using PartnerCommissions.Users.Api.Options;
using PartnerCommissions.Users.Api.Services;
using PartnerCommissions.Users.Domain;
using PartnerCommissions.Users.Infrastructure;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString(UsersOptions.SectionName)
    ?? throw new InvalidOperationException("Database connection string is missing.");
builder.Services.AddDbContext<UsersDbContext>(options => options.UseNpgsql(connectionString));

if (args is ["migrate"])
{
    await using var migrator = builder.Build();
    await using var scope = migrator.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<UsersDbContext>().Database.MigrateAsync();
    return;
}

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.Configure<UsersOptions>(builder.Configuration.GetSection(UsersOptions.SectionName));
builder.Services.AddSingleton<IUtcTime, UtcTime>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUsersAppService, UsersAppService>();
builder.Services.AddGrpc();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
app.MapUsersApi();
app.MapGrpcService<UserInvitersGrpcService>();
app.MapHealthApi();
app.Run();
