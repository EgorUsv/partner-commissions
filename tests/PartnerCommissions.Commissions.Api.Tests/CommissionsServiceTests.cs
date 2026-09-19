using Microsoft.Extensions.Logging.Abstractions;
using PartnerCommissions.Commissions.Api.Application;
using PartnerCommissions.Commissions.Api.Domain;
using PartnerCommissions.Commissions.Api.Hosting;
using PartnerCommissions.Commissions.Api.Infrastructure.Models;

namespace PartnerCommissions.Commissions.Api.Tests;

public sealed class CommissionsServiceTests
{
    private static readonly Guid OwnerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid DirectPartnerId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid SecondPartnerId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid ThirdPartnerId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly DateTimeOffset Now = new(2026, 9, 18, 12, 0, 0, TimeSpan.Zero);

    private static readonly IReadOnlyList<Partner> Chain =
    [
        new(DirectPartnerId, 1),
        new(SecondPartnerId, 2),
        new(ThirdPartnerId, 3)
    ];

    [Fact]
    public async Task Accept_accrues_linear_commissions_for_the_partner_chain()
    {
        var repository = Repository(SchemaType.Linear);
        var service = CreateService(repository);

        var (details, created) = await service.AcceptAsync(Guid.NewGuid(), OwnerId, 100m, CancellationToken.None);

        Assert.True(created);
        Assert.Equal(
            [
                (DirectPartnerId, 1, 1m, SchemaType.Linear),
                (SecondPartnerId, 2, 2m, SchemaType.Linear),
                (ThirdPartnerId, 3, 3m, SchemaType.Linear)
            ],
            details.Commissions.Select(x => (x.PartnerExternalId, x.Level, x.Amount, x.SchemaType)));
        Assert.DoesNotContain(details.Commissions, x => x.PartnerExternalId == OwnerId);
        Assert.All(details.Commissions, x => Assert.Null(x.PaidAt));
        await repository.Received(1).AddEventAsync(
            Arg.Any<Event>(),
            Arg.Is<IReadOnlyList<Commission>>(x => x.Count == 3),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Accept_accrues_fibonacci_commissions_for_the_partner_chain()
    {
        var service = CreateService(Repository(SchemaType.Fibonacci));

        var (details, created) = await service.AcceptAsync(Guid.NewGuid(), OwnerId, 100m, CancellationToken.None);

        Assert.True(created);
        Assert.Equal(
            [
                (DirectPartnerId, 1, 1m, SchemaType.Fibonacci),
                (SecondPartnerId, 2, 1m, SchemaType.Fibonacci),
                (ThirdPartnerId, 3, 2m, SchemaType.Fibonacci)
            ],
            details.Commissions.Select(x => (x.PartnerExternalId, x.Level, x.Amount, x.SchemaType)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-40)]
    public async Task Accept_stores_the_event_without_commissions_when_profit_is_not_positive(decimal profit)
    {
        var repository = Repository(SchemaType.Linear);
        var service = CreateService(repository);
        var operationId = Guid.NewGuid();

        var (details, created) = await service.AcceptAsync(operationId, OwnerId, profit, CancellationToken.None);

        Assert.True(created);
        Assert.Equal(profit, details.Event.Profit);
        Assert.Empty(details.Commissions);
        await repository.Received(1).AddEventAsync(
            Arg.Is<Event>(x => x.OperationId == operationId && x.Profit == profit),
            Arg.Is<IReadOnlyList<Commission>>(x => x.Count == 0),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Accept_keeps_schema_type_on_already_accrued_commissions()
    {
        var repository = Repository(SchemaType.Linear);
        repository.GetSchemaAsync(Arg.Any<CancellationToken>()).Returns(SchemaType.Linear, SchemaType.Fibonacci);
        var saved = new List<IReadOnlyList<Commission>>();
        repository
            .When(x => x.AddEventAsync(
                Arg.Any<Event>(),
                Arg.Any<IReadOnlyList<Commission>>(),
                Arg.Any<CancellationToken>()))
            .Do(call => saved.Add(call.ArgAt<IReadOnlyList<Commission>>(1)));
        var service = CreateService(repository);

        await service.AcceptAsync(Guid.NewGuid(), OwnerId, 100m, CancellationToken.None);
        await service.AcceptAsync(Guid.NewGuid(), OwnerId, 100m, CancellationToken.None);

        Assert.All(saved[0], x => Assert.Equal(SchemaType.Linear, x.SchemaType));
        Assert.Equal([1m, 2m, 3m], saved[0].Select(x => x.Amount));
        Assert.All(saved[1], x => Assert.Equal(SchemaType.Fibonacci, x.SchemaType));
        Assert.Equal([1m, 1m, 2m], saved[1].Select(x => x.Amount));
    }

    [Fact]
    public async Task Accept_response_rounds_money_to_numeric_18_8()
    {
        var service = CreateService(Repository(SchemaType.Linear));

        var (details, created) = await service.AcceptAsync(
            Guid.NewGuid(),
            OwnerId,
            1.23456789m,
            CancellationToken.None);

        Assert.True(created);
        Assert.Equal(1.23456789m, details.Event.Profit);
        Assert.Equal(
            [0.01234568m, 0.02469136m, 0.03703704m],
            details.Commissions.Select(x => x.Amount));
    }

    [Fact]
    public async Task Accept_replay_returns_the_existing_event_without_accruing_again()
    {
        var operationId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var existing = new EventDetails(
            new ProfitEvent(operationId, OwnerId, 100m, Now),
            [
                new AccruedCommission(
                    Guid.NewGuid(),
                    operationId,
                    DirectPartnerId,
                    1,
                    1m,
                    SchemaType.Linear,
                    PaidAt: null)
            ]);
        var repository = Substitute.For<ICommissionRepository>();
        repository.FindEventIdAsync(operationId, Arg.Any<CancellationToken>()).Returns(eventId);
        repository.GetDetailsAsync(eventId, Arg.Any<CancellationToken>()).Returns(existing);
        var service = CreateService(repository);

        var (details, created) = await service.AcceptAsync(operationId, OwnerId, 999m, CancellationToken.None);

        Assert.False(created);
        Assert.Same(existing, details);
        await repository.DidNotReceive().AddEventAsync(
            Arg.Any<Event>(),
            Arg.Any<IReadOnlyList<Commission>>(),
            Arg.Any<CancellationToken>());
    }

    private static ICommissionRepository Repository(SchemaType schema)
    {
        var repository = Substitute.For<ICommissionRepository>();
        repository.GetSchemaAsync(Arg.Any<CancellationToken>()).Returns(schema);
        return repository;
    }

    private static CommissionsService CreateService(ICommissionRepository repository)
    {
        var partners = Substitute.For<IPartnerLookup>();
        partners.GetByOwnerAsync(OwnerId, Arg.Any<CancellationToken>()).Returns(Chain);

        var clock = Substitute.For<IUtcTime>();
        clock.Now.Returns(Now);

        return new CommissionsService(
            repository,
            partners,
            clock,
            new CommissionCalculator(),
            Microsoft.Extensions.Options.Options.Create(new CommissionsOptions()),
            NullLogger<CommissionsService>.Instance,
            Substitute.For<ICommissionsMetrics>());
    }
}
