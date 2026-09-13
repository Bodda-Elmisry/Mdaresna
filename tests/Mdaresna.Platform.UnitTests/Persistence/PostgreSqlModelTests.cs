using Mdaresna.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Text.RegularExpressions;

namespace Mdaresna.Platform.UnitTests.Persistence;

public sealed class PostgreSqlModelTests
{
    [Fact]
    public void Platform_and_identity_models_translate_sql_server_schema_expressions()
    {
        using var platform = new PostgreSqlPlatformDbContext(
            new DbContextOptionsBuilder<PostgreSqlPlatformDbContext>()
                .UseNpgsql("Host=localhost;Database=translation_only;Username=postgres")
                .Options);
        using var identity = new PostgreSqlIdentityDbContext(
            new DbContextOptionsBuilder<PostgreSqlIdentityDbContext>()
                .UseNpgsql("Host=localhost;Database=translation_only_identity;Username=postgres")
                .Options);

        Assert.All(platform.GetService<IDesignTimeModel>().Model.GetEntityTypes()
            .Concat(identity.GetService<IDesignTimeModel>().Model.GetEntityTypes()), entity =>
        {
            foreach (var check in entity.GetCheckConstraints())
                AssertPostgreSqlExpression(check.Sql);
            foreach (var index in entity.GetIndexes())
            {
                if (index.GetFilter() is { } filter)
                    AssertPostgreSqlExpression(filter);
            }
        });
    }

    [Fact]
    public void PostgreSql_model_keeps_billing_invariants()
    {
        using var platform = new PostgreSqlPlatformDbContext(
            new DbContextOptionsBuilder<PostgreSqlPlatformDbContext>()
                .UseNpgsql("Host=localhost;Database=translation_only;Username=postgres")
                .Options);

        var unitType = platform.GetService<IDesignTimeModel>().Model.FindEntityType(
            typeof(Mdaresna.Platform.Domain.Billing.Units.UnitType));
        Assert.NotNull(unitType);
        Assert.Contains(unitType.GetCheckConstraints(),
            check => check.Name == "ck_billing_unit_type_currency" &&
                     check.Sql.Contains("~ '^[A-Z]{3}$'", StringComparison.Ordinal));
    }

    private static void AssertPostgreSqlExpression(string expression)
    {
        Assert.DoesNotContain("DATEPART", expression, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Latin1_General", expression, StringComparison.OrdinalIgnoreCase);
        Assert.False(Regex.IsMatch(expression, @"\[[A-Za-z][A-Za-z0-9_]*\]"), expression);
    }
}
