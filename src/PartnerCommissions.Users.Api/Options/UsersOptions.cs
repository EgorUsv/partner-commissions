namespace PartnerCommissions.Users.Api.Options;

public sealed class UsersOptions
{
    public const string SectionName = "Users";

    public required int MaxTreeDepth { get; init; }
}
