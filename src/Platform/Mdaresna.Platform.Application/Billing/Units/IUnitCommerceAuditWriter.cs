using Mdaresna.Platform.Domain.Access;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Application.Billing.Units;

public sealed record UnitCommerceAuditRecord(
    Guid Id,
    IdentityAccountId ActorId,
    TenantId? TenantId,
    string Action,
    string ResourceType,
    string ResourceId,
    DateTimeOffset OccurredAtUtc,
    Guid CorrelationId,
    string? MetadataJson);

public interface IUnitCommerceAuditWriter
{
    void Stage(UnitCommerceAuditRecord record);
}
