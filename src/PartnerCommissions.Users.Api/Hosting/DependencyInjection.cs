using Microsoft.EntityFrameworkCore;
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
        builder.Services.AddSingleton<IUtcTime, UtcTime>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IUsersService, UsersService>();
        builder.Services.AddGrpc();
        builder.Services.AddExceptionHandler<DomainExceptionHandler>();
        builder.Services.AddProblemDetails();
        return builder;
    }

    public static async Task MigrateAsync(this WebApplicationBuilder builder)
    {
        await using var app = builder.Build();
        await using var scope = app.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<UsersDbContext>().Database.MigrateAsync();
    }
}
