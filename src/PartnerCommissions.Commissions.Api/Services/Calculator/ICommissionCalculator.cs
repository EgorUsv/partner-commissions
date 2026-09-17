using PartnerCommissions.Commissions.Domain;

namespace PartnerCommissions.Commissions.Api.Services.Calculator;

public interface ICommissionCalculator
{
    decimal Amount(SchemaType schema, int level, decimal profit);
}
