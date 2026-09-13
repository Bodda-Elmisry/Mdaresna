using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Mdaresna.Platform.Infrastructure.Persistence;

/// <summary>
/// Translates the existing SQL Server schema rules for PostgreSQL model creation.
/// This runs only for Npgsql; the established SQL Server model and migrations stay intact.
/// </summary>
internal static partial class PostgreSqlModelAdapter
{
    public static void Apply(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var check in entity.GetCheckConstraints().ToArray())
            {
                ((IMutableEntityType)entity).RemoveCheckConstraint(check.ModelName);
                ((IMutableEntityType)entity).AddCheckConstraint(check.ModelName, Translate(check.Sql));
            }

            foreach (var index in entity.GetIndexes())
            {
                if (index.GetFilter() is { } filter)
                    ((IMutableIndex)index).SetFilter(Translate(filter));
            }

            foreach (var property in entity.GetProperties())
            {
                if (property.GetColumnType() == "nvarchar(max)")
                    ((IMutableProperty)property).SetColumnType("text");
            }
        }
    }

    internal static string Translate(string source)
    {
        var sql = UtcOffsetPattern().Replace(source, "TRUE");
        sql = BtrimPattern().Replace(sql, "btrim($1)");
        sql = Regex.Replace(sql, @"\bLEN\(", "length(", RegexOptions.IgnoreCase);
        sql = Regex.Replace(sql, @"\bN'", "'", RegexOptions.IgnoreCase);
        sql = Regex.Replace(sql, @"\[([A-Za-z][A-Za-z0-9_]*)\]", "\"$1\"");
        sql = Regex.Replace(sql, @"\s+COLLATE Latin1_General_BIN2\b", "", RegexOptions.IgnoreCase);
        sql = sql.Replace("LIKE '[A-Z][A-Z][A-Z]'", "~ '^[A-Z]{3}$'", StringComparison.Ordinal);
        sql = BooleanComparisonPattern().Replace(sql, match =>
            $"\"{match.Groups[1].Value}\" = {(match.Groups[2].Value == "1" ? "TRUE" : "FALSE")}");

        if (Regex.IsMatch(sql, @"\[[A-Za-z][A-Za-z0-9_]*\]") ||
            sql.Contains("DATEPART", StringComparison.OrdinalIgnoreCase) ||
            sql.Contains("Latin1_General", StringComparison.OrdinalIgnoreCase) ||
            sql.Contains("LEN(", StringComparison.OrdinalIgnoreCase) ||
            sql.Contains("LTRIM", StringComparison.OrdinalIgnoreCase) ||
            sql.Contains("RTRIM", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"An unported SQL Server schema expression was found: {source} => {sql}");
        }

        return sql;
    }

    [GeneratedRegex(@"DATEPART\(TZOFFSET,\s*\[[A-Za-z][A-Za-z0-9_]*\]\)\s*=\s*0", RegexOptions.IgnoreCase)]
    private static partial Regex UtcOffsetPattern();

    [GeneratedRegex(@"LTRIM\(RTRIM\((\[[A-Za-z][A-Za-z0-9_]*\])\)\)", RegexOptions.IgnoreCase)]
    private static partial Regex BtrimPattern();

    [GeneratedRegex("\\\"(IsVerified|IsPrimary|IsEnabled|IsDeleted|IsActive)\\\"\\s*=\\s*([01])", RegexOptions.IgnoreCase)]
    private static partial Regex BooleanComparisonPattern();
}
