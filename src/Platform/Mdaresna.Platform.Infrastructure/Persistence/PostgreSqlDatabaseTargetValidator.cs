using Npgsql;

namespace Mdaresna.Platform.Infrastructure.Persistence;

internal static class PostgreSqlDatabaseTargetValidator
{
    public static void EnsureDifferent(string platformConnection, string identityConnection)
    {
        var platform = Parse(platformConnection, "PlatformConnection");
        var identity = Parse(identityConnection, "IdentityConnection");
        if (platform == identity)
            throw new InvalidOperationException(
                "PlatformConnection and IdentityConnection must target different PostgreSQL databases.");
    }

    private static (string Host, int Port, string Database) Parse(string connection, string name)
    {
        try
        {
            var builder = new NpgsqlConnectionStringBuilder(connection);
            if (string.IsNullOrWhiteSpace(builder.Host) || string.IsNullOrWhiteSpace(builder.Database))
                throw new ArgumentException();
            return (builder.Host.Trim().ToUpperInvariant(), builder.Port,
                builder.Database.Trim().ToUpperInvariant());
        }
        catch (ArgumentException)
        {
            throw new InvalidOperationException(
                $"ConnectionStrings:{name} must be a PostgreSQL connection string with explicit host and database.");
        }
    }
}
