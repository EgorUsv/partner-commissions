namespace PartnerCommissions.Commissions.Api.Domain;

internal static class Money
{
    public static decimal ToMoney(this decimal value)
    {
        return decimal.Round(value, 8, MidpointRounding.AwayFromZero);
    }
}
