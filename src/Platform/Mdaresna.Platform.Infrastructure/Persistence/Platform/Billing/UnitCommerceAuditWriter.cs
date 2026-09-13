using Mdaresna.Platform.Application.Billing.Units;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Billing;

internal sealed class UnitCommerceAuditWriter(PlatformDbContext dbContext) : IUnitCommerceAuditWriter
{
    public void Stage(UnitCommerceAuditRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        dbContext.AuditEntries.Add(new PlatformAuditEntry
        {
            Id = record.Id,
            AccountId = record.ActorId,
            TenantId = record.TenantId,
            Action = record.Action,
            ResourceType = record.ResourceType,
            ResourceId = record.ResourceId,
            OccurredAtUtc = record.OccurredAtUtc,
            CorrelationId = record.CorrelationId.ToString("D"),
            MetadataJson = record.MetadataJson
        });
    }
}
