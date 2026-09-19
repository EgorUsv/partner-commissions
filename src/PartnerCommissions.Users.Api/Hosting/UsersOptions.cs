namespace PartnerCommissions.Users.Api.Hosting;

public sealed class UsersOptions
{
    public const string SectionName = "Users";

    public required int MaxTreeDepth { get; init; }
}
