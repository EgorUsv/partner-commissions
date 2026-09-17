using PartnerCommissions.Commissions.Domain;

namespace PartnerCommissions.Commissions.Infrastructure.Models;

public sealed class SchemaSetting
{
    public SchemaType SchemaType { get; set; }
    public bool Enabled { get; set; }
}
