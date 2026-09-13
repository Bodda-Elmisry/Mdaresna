using Mdaresna.Tenancy.Abstractions.Identifiers;
using Mdaresna.Platform.Domain.Access;

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;

public sealed class ExternalIdentifierMapping
{
    public Guid Id { get; set; }
    public string SourceSystem { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string ExternalId { get; set; } = string.Empty;
    public Guid InternalId { get; set; }
    public TenantId? TenantId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}

public sealed class PlatformPermissionRecord
{
    public PermissionCode Code { get; set; }
    public string? Description { get; set; }
}

public sealed class PlatformRolePermissionRecord
{
    public PlatformRoleId RoleId { get; set; }
    public PermissionCode PermissionCode { get; set; }
}

public sealed class PlatformAuditEntry
{
    public Guid Id { get; set; }
    public IdentityAccountId? AccountId { get; set; }
    public TenantId? TenantId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public string? ResourceId { get; set; }
    public DateTimeOffset OccurredAtUtc { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? CorrelationId { get; set; }
    public string? MetadataJson { get; set; }
}

public sealed class PlatformFeatureFlag
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public TenantId? TenantId { get; set; }
    public bool IsEnabled { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public IdentityAccountId? UpdatedByAccountId { get; set; }
    public byte[] RowVersion { get; set; } = [];
}

public sealed class PlatformOutboxMessage
{
    public Guid Id { get; set; }
    public string MessageType { get; set; } = string.Empty;
    public ushort SchemaVersion { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
    public DateTimeOffset OccurredAtUtc { get; set; }
    public DateTimeOffset? ProcessedAtUtc { get; set; }
    public DateTimeOffset? NextAttemptAtUtc { get; set; }
    public int AttemptCount { get; set; }
    public string? LastError { get; set; }
    public Guid CorrelationId { get; set; }
    public Guid? CausationId { get; set; }
    public string? TraceParent { get; set; }
    public byte[] RowVersion { get; set; } = [];
}

public sealed class PlatformInboxMessage
{
    public Guid MessageId { get; set; }
    public string Consumer { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public ushort SchemaVersion { get; set; }
    public DateTimeOffset ReceivedAtUtc { get; set; }
    public DateTimeOffset? ProcessedAtUtc { get; set; }
    public int AttemptCount { get; set; }
    public string? LastError { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
