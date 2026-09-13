namespace Mdaresna.Platform.Infrastructure.Persistence.DesignTime;

internal static class DesignTimeConnectionStrings
{
    private const string PlatformEnvironmentVariable =
        "MDARESNA_PLATFORM_DESIGNTIME_CONNECTION";

    private const string IdentityEnvironmentVariable =
        "MDARESNA_IDENTITY_DESIGNTIME_CONNECTION";

    private const string PlatformLocalConnection =
        "Server=(localdb)\\MSSQLLocalDB;Database=MdaresnaPlatformLocal;" +
        "Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True;";

    private const string IdentityLocalConnection =
        "Server=(localdb)\\MSSQLLocalDB;Database=MdaresnaIdentityLocal;" +
        "Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True;";

    public static string Platform => GetOrUseLocal(
        PlatformEnvironmentVariable,
        PlatformLocalConnection);

    public static string Identity => GetOrUseLocal(
        IdentityEnvironmentVariable,
        IdentityLocalConnection);

    private static string GetOrUseLocal(string environmentVariable, string localConnection)
    {
        var configured = Environment.GetEnvironmentVariable(environmentVariable);
        return string.IsNullOrWhiteSpace(configured) ? localConnection : configured;
    }
}
