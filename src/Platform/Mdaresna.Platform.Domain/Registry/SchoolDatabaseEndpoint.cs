using Mdaresna.Platform.Domain.Common;
using Mdaresna.Tenancy.Abstractions.Identifiers;
using System.Text.Json.Serialization;

namespace Mdaresna.Platform.Domain.Registry;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SchoolDatabasePurpose
{
    Operational,
    Reporting,
    Archive,
    ReadReplica
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SchoolDatabaseProvider
{
    PostgreSql,
    SqlServer
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SchoolDatabaseEndpointStatus
{
    Provisioning,
    Active,
    Unavailable,
    Retired
}

/// <summary>
/// Platform control-plane metadata for locating a school's database.
/// Credentials never belong in this aggregate; CredentialSecretReference points
/// to the configured external secret store instead.
/// </summary>
public sealed class SchoolDatabaseEndpoint : AggregateRoot
{
    private SchoolDatabaseEndpoint(
        Guid id,
        SchoolId schoolId,
        SchoolDatabasePurpose purpose,
        SchoolDatabaseProvider provider,
        string host,
        int port,
        string databaseName,
        string credentialSecretReference,
        bool requireTls,
        bool isPrimary,
        SchoolDatabaseEndpointStatus status,
        string? region,
        string? schemaVersion,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc,
        long version)
    {
        Id = id;
        SchoolId = schoolId;
        Purpose = purpose;
        Provider = provider;
        Host = host;
        Port = port;
        DatabaseName = databaseName;
        CredentialSecretReference = credentialSecretReference;
        RequireTls = requireTls;
        IsPrimary = isPrimary;
        Status = status;
        Region = region;
        SchemaVersion = schemaVersion;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
        RestoreVersion(version);
    }

    public Guid Id { get; }
    public SchoolId SchoolId { get; }
    public SchoolDatabasePurpose Purpose { get; }
    public SchoolDatabaseProvider Provider { get; }
    public string Host { get; private set; }
    public int Port { get; private set; }
    public string DatabaseName { get; private set; }
    public string CredentialSecretReference { get; private set; }
    public bool RequireTls { get; private set; }
    public bool IsPrimary { get; private set; }
    public SchoolDatabaseEndpointStatus Status { get; private set; }
    public string? Region { get; private set; }
    public string? SchemaVersion { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static SchoolDatabaseEndpoint Create(
        Guid id,
        SchoolId schoolId,
        SchoolDatabasePurpose purpose,
        SchoolDatabaseProvider provider,
        string host,
        int port,
        string databaseName,
        string credentialSecretReference,
        bool requireTls,
        bool isPrimary,
        string? region,
        string? schemaVersion,
        DateTimeOffset occurredAtUtc)
    {
        DomainGuard.NonEmptyGuid(id, nameof(id));
        EnsureSchoolId(schoolId);
        EnsureEnum(purpose, nameof(purpose));
        EnsureEnum(provider, nameof(provider));
        var now = DomainGuard.UtcTimestamp(occurredAtUtc, nameof(occurredAtUtc));
        return new SchoolDatabaseEndpoint(
            id,
            schoolId,
            purpose,
            provider,
            NormalizeHost(host),
            ValidatePort(port),
            DomainGuard.RequiredText(databaseName, 128, nameof(databaseName)),
            NormalizeSecretReference(credentialSecretReference),
            requireTls,
            isPrimary,
            SchoolDatabaseEndpointStatus.Provisioning,
            DomainGuard.OptionalText(region, 100, nameof(region)),
            DomainGuard.OptionalText(schemaVersion, 64, nameof(schemaVersion)),
            now,
            now,
            version: 0);
    }

    public bool UpdateTarget(
        string host,
        int port,
        string databaseName,
        string credentialSecretReference,
        bool requireTls,
        string? region,
        string? schemaVersion,
        DateTimeOffset occurredAtUtc)
    {
        EnsureNotRetired();
        var normalizedHost = NormalizeHost(host);
        var validatedPort = ValidatePort(port);
        var normalizedDatabaseName = DomainGuard.RequiredText(
            databaseName, 128, nameof(databaseName));
        var normalizedSecretReference = NormalizeSecretReference(credentialSecretReference);
        var normalizedRegion = DomainGuard.OptionalText(region, 100, nameof(region));
        var normalizedSchemaVersion = DomainGuard.OptionalText(
            schemaVersion, 64, nameof(schemaVersion));

        if (Host == normalizedHost && Port == validatedPort &&
            DatabaseName == normalizedDatabaseName &&
            CredentialSecretReference == normalizedSecretReference &&
            RequireTls == requireTls && Region == normalizedRegion &&
            SchemaVersion == normalizedSchemaVersion)
        {
            return false;
        }

        Host = normalizedHost;
        Port = validatedPort;
        DatabaseName = normalizedDatabaseName;
        CredentialSecretReference = normalizedSecretReference;
        RequireTls = requireTls;
        Region = normalizedRegion;
        SchemaVersion = normalizedSchemaVersion;
        Touch(occurredAtUtc);
        return true;
    }

    public bool ChangeStatus(
        SchoolDatabaseEndpointStatus status,
        DateTimeOffset occurredAtUtc)
    {
        EnsureEnum(status, nameof(status));
        if (Status == status)
        {
            return false;
        }

        if (Status == SchoolDatabaseEndpointStatus.Retired)
        {
            throw new PlatformDomainException(
                "school_database_endpoint.retired",
                "A retired school database endpoint cannot be reactivated.");
        }

        Status = status;
        if (status == SchoolDatabaseEndpointStatus.Retired)
        {
            IsPrimary = false;
        }

        Touch(occurredAtUtc);
        return true;
    }

    public bool SetPrimary(bool isPrimary, DateTimeOffset occurredAtUtc)
    {
        EnsureNotRetired();
        if (IsPrimary == isPrimary)
        {
            return false;
        }

        IsPrimary = isPrimary;
        Touch(occurredAtUtc);
        return true;
    }

    internal static SchoolDatabaseEndpoint Rehydrate(
        Guid id,
        SchoolId schoolId,
        SchoolDatabasePurpose purpose,
        SchoolDatabaseProvider provider,
        string host,
        int port,
        string databaseName,
        string credentialSecretReference,
        bool requireTls,
        bool isPrimary,
        SchoolDatabaseEndpointStatus status,
        string? region,
        string? schemaVersion,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc,
        long version)
    {
        DomainGuard.NonEmptyGuid(id, nameof(id));
        EnsureSchoolId(schoolId);
        EnsureEnum(purpose, nameof(purpose));
        EnsureEnum(provider, nameof(provider));
        EnsureEnum(status, nameof(status));
        var created = DomainGuard.UtcTimestamp(createdAtUtc, nameof(createdAtUtc));
        var updated = DomainGuard.UtcTimestamp(updatedAtUtc, nameof(updatedAtUtc));
        if (updated < created)
        {
            throw new ArgumentException("UpdatedAtUtc cannot precede CreatedAtUtc.");
        }

        if (status == SchoolDatabaseEndpointStatus.Retired && isPrimary)
        {
            throw new ArgumentException("A retired endpoint cannot be primary.");
        }

        return new SchoolDatabaseEndpoint(
            id,
            schoolId,
            purpose,
            provider,
            NormalizeHost(host),
            ValidatePort(port),
            DomainGuard.RequiredText(databaseName, 128, nameof(databaseName)),
            NormalizeSecretReference(credentialSecretReference),
            requireTls,
            isPrimary,
            status,
            DomainGuard.OptionalText(region, 100, nameof(region)),
            DomainGuard.OptionalText(schemaVersion, 64, nameof(schemaVersion)),
            created,
            updated,
            version);
    }

    private void Touch(DateTimeOffset occurredAtUtc)
    {
        UpdatedAtUtc = DomainGuard.UtcTimestamp(occurredAtUtc, nameof(occurredAtUtc));
        MarkChanged();
    }

    private void EnsureNotRetired()
    {
        if (Status == SchoolDatabaseEndpointStatus.Retired)
        {
            throw new PlatformDomainException(
                "school_database_endpoint.retired",
                "A retired school database endpoint cannot be changed.");
        }
    }

    private static string NormalizeHost(string host) =>
        DomainGuard.RequiredText(host, 253, nameof(host)).ToLowerInvariant();

    private static string NormalizeSecretReference(string credentialSecretReference)
    {
        var reference = DomainGuard.RequiredText(
            credentialSecretReference, 500, nameof(credentialSecretReference));
        var looksLikeCredential = reference.Contains("Password=", StringComparison.OrdinalIgnoreCase) ||
                                  reference.Contains("Pwd=", StringComparison.OrdinalIgnoreCase) ||
                                  reference.Contains("Username=", StringComparison.OrdinalIgnoreCase) ||
                                  reference.Contains("User Id=", StringComparison.OrdinalIgnoreCase);
        if (looksLikeCredential)
        {
            throw new ArgumentException(
                "CredentialSecretReference must reference an external secret, not contain credentials.",
                nameof(credentialSecretReference));
        }

        return reference;
    }

    private static int ValidatePort(int port) => port is >= 1 and <= 65535
        ? port
        : throw new ArgumentOutOfRangeException(nameof(port), "Port must be between 1 and 65535.");

    private static void EnsureSchoolId(SchoolId schoolId)
    {
        if (schoolId.IsEmpty)
        {
            throw new ArgumentException("SchoolId cannot be empty.", nameof(schoolId));
        }
    }

    private static void EnsureEnum<T>(T value, string parameterName) where T : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(parameterName);
        }
    }
}
