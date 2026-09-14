using Mdaresna.Platform.Infrastructure.Persistence;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;
using Mdaresna.Platform.Infrastructure.Persistence.Identity.Entities;
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

    [Fact]
    public void Local_platform_user_is_separate_from_shared_person_and_owns_credential()
    {
        using var platform = new PostgreSqlPlatformDbContext(
            new DbContextOptionsBuilder<PostgreSqlPlatformDbContext>()
                .UseNpgsql("Host=localhost;Database=translation_only;Username=postgres")
                .Options);
        using var identity = new PostgreSqlIdentityDbContext(
            new DbContextOptionsBuilder<PostgreSqlIdentityDbContext>()
                .UseNpgsql("Host=localhost;Database=translation_only_identity;Username=postgres")
                .Options);

        var localUser = platform.GetService<IDesignTimeModel>().Model
            .FindEntityType(typeof(PlatformLocalUser));
        var credential = platform.GetService<IDesignTimeModel>().Model
            .FindEntityType(typeof(PlatformLocalCredential));
        var person = identity.GetService<IDesignTimeModel>().Model
            .FindEntityType(typeof(Account));

        Assert.NotNull(localUser);
        Assert.NotNull(credential);
        Assert.NotNull(person);
        Assert.Equal("access", localUser.GetSchema());
        Assert.Equal("access", credential.GetSchema());
        Assert.Contains(localUser.GetIndexes(), index =>
            index.IsUnique && index.Properties.Single().Name == nameof(PlatformLocalUser.PersonId));
        Assert.Contains(localUser.GetIndexes(), index =>
            index.IsUnique && index.Properties.Single().Name == nameof(PlatformLocalUser.NormalizedUserName));
        Assert.Contains(credential.GetForeignKeys(), foreignKey =>
            foreignKey.PrincipalEntityType == localUser);
        Assert.Null(person.FindProperty("PasswordHash"));
        Assert.NotNull(person.FindProperty(nameof(Account.DateOfBirth)));
        Assert.NotNull(person.FindProperty(nameof(Account.GenderCode)));
    }

    [Fact]
    public void Account_contacts_are_stored_with_the_shared_identity_account()
    {
        using var identity = new PostgreSqlIdentityDbContext(
            new DbContextOptionsBuilder<PostgreSqlIdentityDbContext>()
                .UseNpgsql("Host=localhost;Database=translation_only_identity;Username=postgres")
                .Options);

        var contact = identity.GetService<IDesignTimeModel>().Model
            .FindEntityType(typeof(AccountContact));
        Assert.NotNull(contact);
        Assert.Equal("identity", contact.GetSchema());
        Assert.Equal("account_contacts", contact.GetTableName());
        Assert.Contains(contact.GetForeignKeys(), foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(Account));
        var identifier = identity.GetService<IDesignTimeModel>().Model
            .FindEntityType(typeof(LoginIdentifier));
        Assert.NotNull(identifier);
        Assert.Contains(identifier.GetIndexes(), index =>
            index.IsUnique && index.Properties.Select(property => property.Name)
                .SequenceEqual(new[] { nameof(LoginIdentifier.AccountId), nameof(LoginIdentifier.Type) }) &&
            index.GetFilter()?.Contains("IsPrimary", StringComparison.Ordinal) == true);
    }

    private static void AssertPostgreSqlExpression(string expression)
    {
        Assert.DoesNotContain("DATEPART", expression, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Latin1_General", expression, StringComparison.OrdinalIgnoreCase);
        Assert.False(Regex.IsMatch(expression, @"\[[A-Za-z][A-Za-z0-9_]*\]"), expression);
    }
}
