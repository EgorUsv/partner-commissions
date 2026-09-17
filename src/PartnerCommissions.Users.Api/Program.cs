using Microsoft.EntityFrameworkCore;
using PartnerCommissions.Users.Api.Endpoints;
using PartnerCommissions.Users.Api.Grpc;
using PartnerCommissions.Users.Api.Hosting;
using PartnerCommissions.Users.Api.Options;
using PartnerCommissions.Users.Api.Services;
using PartnerCommissions.Users.Domain;
using PartnerCommissions.Users.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<UsersOptions>(builder.Configuration.GetSection(UsersOptions.SectionName));

var connectionString = builder.Configuration.GetConnectionString(UsersOptions.SectionName)
    ?? throw new InvalidOperationException("Database connection string is missing.");
builder.Services.AddDbContext<UsersDbContext>(options => options.UseNpgsql(connectionString));
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
