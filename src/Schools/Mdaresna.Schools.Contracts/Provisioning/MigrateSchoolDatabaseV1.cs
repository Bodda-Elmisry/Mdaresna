using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Schools.Contracts.Provisioning;

public sealed record MigrateSchoolDatabaseV1(
    Guid OperationId,
    TenantId TenantId,
    SchoolId SchoolId,
    string DatabaseName) : IIntegrationCommand
{
    public static string MessageType => "mdaresna.schools.database.migrate";
    public static ushort SchemaVersion => 1;
}

public sealed record SchoolDatabaseMigratedV1(
    Guid OperationId,
    TenantId TenantId,
    SchoolId SchoolId,
    bool Succeeded,
    string? PreviousSchemaVersion,
    string? CurrentSchemaVersion,
    string? Error,
    DateTimeOffset CompletedAtUtc) : IIntegrationEvent
{
    public static string MessageType => "mdaresna.schools.database.migrated";
    public static ushort SchemaVersion => 1;
}
