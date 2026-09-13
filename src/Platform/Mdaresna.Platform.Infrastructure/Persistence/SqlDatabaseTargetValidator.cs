using Microsoft.Data.SqlClient;

namespace Mdaresna.Platform.Infrastructure.Persistence;

internal static class SqlDatabaseTargetValidator
{
    public static void EnsureDifferent(string platformConnection, string identityConnection)
    {
        var platformTarget = Parse(platformConnection, "PlatformConnection");
        var identityTarget = Parse(identityConnection, "IdentityConnection");

        if (platformTarget == identityTarget)
        {
            throw new InvalidOperationException(
                "PlatformConnection and IdentityConnection must target different SQL Server databases.");
        }
    }

    private static SqlDatabaseTarget Parse(string connectionString, string connectionName)
    {
        try
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            var server = NormalizeServer(builder.DataSource);
            var catalog = builder.InitialCatalog.Trim();
            var attachedFile = builder.AttachDBFilename.Trim();

            if (string.IsNullOrWhiteSpace(server) ||
                (string.IsNullOrWhiteSpace(catalog) && string.IsNullOrWhiteSpace(attachedFile)))
            {
                throw new ArgumentException();
            }

            var database = string.IsNullOrWhiteSpace(catalog)
                ? $"file:{attachedFile}"
                : $"catalog:{catalog}";

            return new SqlDatabaseTarget(
                server.ToUpperInvariant(),
                database.ToUpperInvariant());
        }
        catch (ArgumentException)
        {
            throw new InvalidOperationException(
                $"ConnectionStrings:{connectionName} must be a valid SQL Server connection string " +
                "with an explicit database target.");
        }
    }

    private static string NormalizeServer(string server)
    {
        var normalized = server.Trim();
        var separatorIndex = normalized.IndexOf('\\');
        var host = separatorIndex < 0 ? normalized : normalized[..separatorIndex];
        var suffix = separatorIndex < 0 ? string.Empty : normalized[separatorIndex..];

        if (host is "." or "(local)" ||
            host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
        {
            host = "localhost";
        }

        return host + suffix;
    }

    private readonly record struct SqlDatabaseTarget(string Server, string Database);
}
