using Mdaresna.Platform.Domain.Access;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Application.Registry.Lifecycle;

public sealed record RegistryAuditRecord(
    Guid Id,
    IdentityAccountId ActorId,
    TenantId TenantId,
    string Action,
    string ResourceType,
    string ResourceId,
    DateTimeOffset OccurredAtUtc,
    Guid CorrelationId,
    string? MetadataJson);

public interface IPlatformRegistryAuditWriter
{
    void Stage(RegistryAuditRecord record);
}
