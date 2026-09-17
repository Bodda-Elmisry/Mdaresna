using System.Text.Json.Serialization;
using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Schools.Contracts.Provisioning;

public sealed record SchoolProvisionedV1(
    [property: JsonPropertyName("operationId")] Guid OperationId,
    [property: JsonPropertyName("tenantId")] TenantId TenantId,
    [property: JsonPropertyName("schoolId")] SchoolId SchoolId,
    [property: JsonPropertyName("localSchoolId")] Guid LocalSchoolId,
    [property: JsonPropertyName("provider")] string Provider,
    [property: JsonPropertyName("host")] string Host,
    [property: JsonPropertyName("port")] int Port,
    [property: JsonPropertyName("databaseName")] string DatabaseName,
    [property: JsonPropertyName("credentialSecretReference")] string CredentialSecretReference,
    [property: JsonPropertyName("requireTls")] bool RequireTls,
    [property: JsonPropertyName("databaseSchemaVersion")] string DatabaseSchemaVersion,
    [property: JsonPropertyName("completedAtUtc")] DateTimeOffset CompletedAtUtc,
    [property: JsonPropertyName("ownerFullUserName")] string OwnerFullUserName) : IIntegrationEvent
{
    public static string MessageType => "mdaresna.schools.provisioning.school-provisioned";
    public static ushort SchemaVersion => 3;
}

/// <summary>Read-only compatibility shape for provisioning results already queued before schema 3.</summary>
[Obsolete("Only use when consuming an already-queued schema 2 message. Never publish this contract.")]
public sealed record SchoolProvisionedV2(
    [property: JsonPropertyName("operationId")] Guid OperationId,
    [property: JsonPropertyName("tenantId")] TenantId TenantId,
    [property: JsonPropertyName("schoolId")] SchoolId SchoolId,
    [property: JsonPropertyName("localSchoolId")] Guid LocalSchoolId,
    [property: JsonPropertyName("provider")] string Provider,
    [property: JsonPropertyName("host")] string Host,
    [property: JsonPropertyName("port")] int Port,
    [property: JsonPropertyName("databaseName")] string DatabaseName,
    [property: JsonPropertyName("credentialSecretReference")] string CredentialSecretReference,
    [property: JsonPropertyName("requireTls")] bool RequireTls,
    [property: JsonPropertyName("databaseSchemaVersion")] string DatabaseSchemaVersion,
    [property: JsonPropertyName("completedAtUtc")] DateTimeOffset CompletedAtUtc,
    [property: JsonPropertyName("ownerFullUserName")] string OwnerFullUserName,
    [property: JsonPropertyName("ownerActivationCode")] string OwnerActivationCode) : IIntegrationEvent
{
    public static string MessageType => SchoolProvisionedV1.MessageType;
    public static ushort SchemaVersion => 2;
}
