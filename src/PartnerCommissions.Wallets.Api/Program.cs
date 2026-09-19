using PartnerCommissions.Wallets.Api.Endpoints;
using PartnerCommissions.Wallets.Api.Hosting;

var builder = WebApplication.CreateBuilder(args);
builder.AddApi();

if (args is ["migrate"])
{
    await builder.MigrateAsync();
    return;
}

var app = builder.Build();
app.UseExceptionHandler();
app.MapWalletsApi();
app.MapHealthApi();
app.Run();
