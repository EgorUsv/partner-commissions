using PartnerCommissions.Commissions.Api.Domain;

namespace PartnerCommissions.Commissions.Api.Infrastructure.Models;

public sealed class SchemaSetting
{
    public SchemaType SchemaType { get; set; }
    public bool Enabled { get; set; }
}
