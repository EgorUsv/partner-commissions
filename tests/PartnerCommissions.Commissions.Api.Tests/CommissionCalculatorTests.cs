using PartnerCommissions.Commissions.Api.Domain;

namespace PartnerCommissions.Commissions.Api.Tests;

public sealed class CommissionCalculatorTests
{
    private readonly CommissionCalculator calculator = new();

    [Theory]
    [InlineData(1, 100, 1)]
    [InlineData(2, 100, 2)]
    [InlineData(3, 100, 3)]
    [InlineData(10, 100, 10)]
    [InlineData(1, 250, 2.5)]
    public void Linear_is_level_times_profit_over_100(int level, decimal profit, decimal expected)
    {
        Assert.Equal(expected, calculator.Amount(SchemaType.Linear, level, profit));
    }

    [Fact]
    public void Linear_keeps_decimal_precision()
    {
        Assert.Equal(0.6666m, calculator.Amount(SchemaType.Linear, 2, 33.33m));
    }

    [Theory]
    [InlineData(1, 100, 1)]
    [InlineData(2, 100, 1)]
    [InlineData(3, 100, 2)]
    [InlineData(4, 100, 3)]
    [InlineData(5, 100, 5)]
    [InlineData(6, 100, 8)]
    [InlineData(10, 100, 55)]
    [InlineData(3, 250, 5)]
    public void Fibonacci_is_F_level_times_profit_over_100(int level, decimal profit, decimal expected)
    {
        Assert.Equal(expected, calculator.Amount(SchemaType.Fibonacci, level, profit));
    }

    [Fact]
    public void Direct_partner_is_level_one()
    {
        const decimal profit = 100m;

        Assert.Equal(1m, calculator.Amount(SchemaType.Linear, 1, profit));
        Assert.Equal(1m, calculator.Amount(SchemaType.Fibonacci, 1, profit));
    }

    [Fact]
    public void Schemas_diverge_from_level_three()
    {
        const decimal profit = 100m;

        Assert.Equal(3m, calculator.Amount(SchemaType.Linear, 3, profit));
        Assert.Equal(2m, calculator.Amount(SchemaType.Fibonacci, 3, profit));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Level_must_be_at_least_one(int level)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => calculator.Amount(SchemaType.Linear, level, 100m));
    }
}
