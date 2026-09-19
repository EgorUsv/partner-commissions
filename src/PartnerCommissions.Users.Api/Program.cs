using PartnerCommissions.Users.Api.Endpoints;
using PartnerCommissions.Users.Api.Grpc;
using PartnerCommissions.Users.Api.Hosting;

var builder = WebApplication.CreateBuilder(args);
builder.AddApi();

if (args is ["migrate"])
{
    await builder.MigrateAsync();
    return;
}

var app = builder.Build();
app.UseExceptionHandler();
app.MapUsersApi();
app.MapGrpcService<UserInvitersGrpcService>();
app.MapHealthApi();
app.MapPrometheusScrapingEndpoint();
app.Run();
