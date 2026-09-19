using OpenTelemetry.Metrics;
using PartnerCommissions.Commissions.Api.Endpoints;
using PartnerCommissions.Commissions.Api.Grpc;
using PartnerCommissions.Commissions.Api.Hosting;

var builder = WebApplication.CreateBuilder(args);
builder.AddApi();

if (args is ["migrate"])
{
    await builder.MigrateAsync();
    return;
}

var app = builder.Build();
app.UseExceptionHandler();
app.MapEventsApi();
app.MapSchemaApi();
app.MapGrpcService<CommissionPayoutsGrpcService>();
app.MapHealthApi();
app.MapPrometheusScrapingEndpoint();
app.Run();
