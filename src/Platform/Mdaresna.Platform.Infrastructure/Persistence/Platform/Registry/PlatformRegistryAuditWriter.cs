using Mdaresna.Platform.Application.Registry.Lifecycle;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Registry;

internal sealed class PlatformRegistryAuditWriter(PlatformDbContext dbContext) : IPlatformRegistryAuditWriter
{
    public void Stage(RegistryAuditRecord record)
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
