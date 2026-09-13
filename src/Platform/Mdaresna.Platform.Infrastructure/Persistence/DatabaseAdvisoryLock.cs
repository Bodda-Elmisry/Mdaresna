using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Mdaresna.Platform.Infrastructure.Persistence;

public static class DatabaseAdvisoryLock
{
    public static async Task AcquireAsync(
        DbContext db, IDbContextTransaction transaction, string resource,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resource);
        var connection = db.Database.GetDbConnection();
        await using var command = connection.CreateCommand();
        command.Transaction = transaction.GetDbTransaction();
        command.CommandTimeout = 5;
        var parameter = command.CreateParameter();
        parameter.ParameterName = "resource";
        parameter.Value = resource;
        command.Parameters.Add(parameter);

        if (db.Database.IsNpgsql())
        {
            command.CommandText = "SELECT pg_try_advisory_xact_lock(hashtextextended(@resource, 0))";
            if (await command.ExecuteScalarAsync(cancellationToken) is not true)
                throw new InvalidOperationException("The requested database operation is busy.");
        }
        else if (db.Database.IsSqlServer())
        {
            command.CommandText = "DECLARE @result int; " +
                                  "EXEC @result = sys.sp_getapplock " +
                                  "@Resource = @resource, @LockMode = 'Exclusive', " +
                                  "@LockOwner = 'Transaction', @LockTimeout = 5000; " +
                                  "SELECT @result;";
            if (await command.ExecuteScalarAsync(cancellationToken) is not int result || result < 0)
                throw new InvalidOperationException("The requested database operation is busy.");
        }
        else
        {
            throw new NotSupportedException("The database provider does not support advisory locks.");
        }
    }
}
