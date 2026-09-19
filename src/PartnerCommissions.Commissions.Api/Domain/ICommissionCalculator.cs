namespace PartnerCommissions.Commissions.Api.Domain;

public interface ICommissionCalculator
{
    decimal Amount(SchemaType schema, int level, decimal profit);
}
