using Microsoft.Data.SqlClient;
using Npgsql;

namespace Mdaresna.Platform.Infrastructure.Persistence;

internal static class DatabaseErrorClassifier
{
    public static bool IsUniqueViolation(Exception? exception) => exception switch
    {
        SqlException { Number: 2601 or 2627 } => true,
        PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } => true,
        _ => false
    };
}
