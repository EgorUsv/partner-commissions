using PartnerCommissions.Commissions.Domain;

namespace PartnerCommissions.Commissions.Api.Services.Calculator;

public sealed class CommissionCalculator : ICommissionCalculator
{
    private const int MaxFibonacciLevel = 46;

    public decimal Amount(SchemaType schema, int level, decimal profit)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(level, 1);
        var coefficient = schema switch
        {
            SchemaType.Linear => level,
            SchemaType.Fibonacci => Fibonacci(level),
            _ => throw new ArgumentOutOfRangeException(nameof(schema))
        };
        return coefficient * profit / 100m;
    }

    private static int Fibonacci(int n)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(n, MaxFibonacciLevel);
        if (n <= 2)
        {
            return 1;
        }

        var previous = 1;
        var current = 1;
        for (var i = 3; i <= n; i++)
        {
            var next = previous + current;
            previous = current;
            current = next;
        }

        return current;
    }
}
