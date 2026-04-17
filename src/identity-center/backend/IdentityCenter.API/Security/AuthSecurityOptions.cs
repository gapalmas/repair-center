namespace IdentityCenter.API.Security;

public sealed class AuthSecurityOptions
{
    public const string SectionName = "AuthSecurity";

    public List<string> AllowedOrigins { get; init; } = [];
}