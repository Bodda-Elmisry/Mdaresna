using Mdaresna.Platform.Domain.Common;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.UnitTests.Domain;

public sealed class SchoolDatabaseEndpointTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 9, 15, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Endpoint_stores_a_secret_reference_and_rejects_a_connection_string()
    {
        var exception = Assert.Throws<ArgumentException>(() => Create(
            "Host=db;Username=school;Password=unsafe"));

        Assert.Contains("external secret", exception.Message);
    }

    [Fact]
    public void Retired_endpoint_is_not_primary_and_cannot_be_reactivated()
    {
        var endpoint = Create("secret://schools/one/database", isPrimary: true);

        endpoint.ChangeStatus(SchoolDatabaseEndpointStatus.Active, Now.AddMinutes(1));
        endpoint.ChangeStatus(SchoolDatabaseEndpointStatus.Retired, Now.AddMinutes(2));

        Assert.False(endpoint.IsPrimary);
        var exception = Assert.Throws<PlatformDomainException>(() =>
            endpoint.ChangeStatus(SchoolDatabaseEndpointStatus.Active, Now.AddMinutes(3)));
        Assert.Equal("school_database_endpoint.retired", exception.Code);
    }

    [Fact]
    public void Endpoint_target_is_normalized_without_exposing_credentials()
    {
        var endpoint = Create("secret://schools/one/database");

        Assert.Equal("db.internal", endpoint.Host);
        Assert.Equal("school_one", endpoint.DatabaseName);
        Assert.Equal("secret://schools/one/database", endpoint.CredentialSecretReference);
        Assert.Equal(SchoolDatabaseEndpointStatus.Provisioning, endpoint.Status);
        Assert.Equal(SchoolDatabaseMigrationStatus.Succeeded, endpoint.MigrationStatus);
    }

    [Fact]
    public void Active_endpoint_tracks_a_database_migration_to_completion()
    {
        var endpoint = Create("secret://schools/one/database", isPrimary: true);
        endpoint.ChangeStatus(SchoolDatabaseEndpointStatus.Active, Now.AddMinutes(1));
        var operationId = Guid.NewGuid();

        endpoint.QueueMigration(operationId, Now.AddMinutes(2));
        endpoint.CompleteMigration(operationId, true, "20260917165538_latest", null,
            Now.AddMinutes(3));

        Assert.Equal(SchoolDatabaseMigrationStatus.Succeeded, endpoint.MigrationStatus);
        Assert.Equal(operationId, endpoint.LastMigrationOperationId);
        Assert.Equal("20260917165538_latest", endpoint.SchemaVersion);
        Assert.Null(endpoint.LastMigrationError);
    }

    private static SchoolDatabaseEndpoint Create(
        string secretReference,
        bool isPrimary = false) =>
        SchoolDatabaseEndpoint.Create(
            Guid.NewGuid(),
            SchoolId.New(),
            SchoolDatabasePurpose.Operational,
            SchoolDatabaseProvider.PostgreSql,
            "DB.Internal",
            5432,
            "school_one",
            secretReference,
            requireTls: true,
            isPrimary,
            "egypt-north",
            "1.0.0",
            Now);
}
