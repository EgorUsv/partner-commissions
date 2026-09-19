using System.Diagnostics.Metrics;
using Microsoft.Extensions.Options;

namespace PartnerCommissions.Users.Api.Hosting;

public sealed class UsersMetrics(IMeterFactory meters, IOptions<MetricsOptions> options) : IUsersMetrics
{
    private readonly Counter<long> usersCreated = meters
        .Create(options.Value.MeterName)
        .CreateCounter<long>(options.Value.UsersCreated);

    public void UserCreated() => usersCreated.Add(1);
}
