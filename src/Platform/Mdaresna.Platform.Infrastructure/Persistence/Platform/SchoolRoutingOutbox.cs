using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;
using Mdaresna.Schools.Contracts.Provisioning;
using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.IntegrationContracts.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform;

public partial class PlatformDbContext
{
    private async Task PrepareRoutingEventsAsync(bool asynchronous, CancellationToken ct)
    {
        ChangeTracker.DetectChanges();
        var endpointProperties = new[] { "Host", "Port", "DatabaseName", "CredentialSecretReference", "RequireTls", "IsPrimary", "Status", "Provider" };
        var schoolIds = ChangeTracker.Entries<SchoolRegistration>()
            .Where(e => e.State is EntityState.Added or EntityState.Deleted ||
                e.State == EntityState.Modified && (e.Property(x => x.Status).IsModified || e.Property(x => x.Code).IsModified))
            .Select(e => e.Entity.Id)
            .Concat(ChangeTracker.Entries<SchoolDatabaseEndpoint>()
                .Where(e => e.Entity.Purpose == SchoolDatabasePurpose.Operational &&
                    (e.State is EntityState.Added or EntityState.Deleted || e.State == EntityState.Modified &&
                        endpointProperties.Any(p => e.Property(p).IsModified)))
                .Select(e => e.Entity.SchoolId)).Distinct().ToArray();
        foreach (var id in schoolIds)
        {
            var school = ChangeTracker.Entries<SchoolRegistration>().Select(e => e.Entity).FirstOrDefault(s => s.Id == id);
            if (school is null) school = asynchronous ? await Schools.FindAsync([id], ct) : Schools.Find(id);
            if (school is null) continue;
            school.AdvanceRoutingVersion();
            // Tracking queries preserve pending changes, including primary demotions in a transaction.
            var query = SchoolDatabaseEndpoints.Where(e => e.SchoolId == id && e.Purpose == SchoolDatabasePurpose.Operational);
            if (asynchronous) await query.LoadAsync(ct); else query.Load();
            var endpoint = SchoolDatabaseEndpoints.Local.SingleOrDefault(e => e.SchoolId == id &&
                e.Purpose == SchoolDatabasePurpose.Operational && e.IsPrimary && e.Status == SchoolDatabaseEndpointStatus.Active &&
                Entry(e).State != EntityState.Deleted);
            var active = Entry(school).State != EntityState.Deleted && school.Status == SchoolLifecycleStatus.Active && endpoint is not null;
            var target = active ? new SchoolRoutingData(school.TenantId.Value, school.Id.Value, school.Code.Value,
                "Active", endpoint!.Provider.ToString(), endpoint.Host, endpoint.Port, endpoint.DatabaseName,
                endpoint.CredentialSecretReference, endpoint.RequireTls, school.RoutingVersion) : null;
            var message = IntegrationMessageEnvelope<SchoolRoutingChangedV1>.Create(DateTimeOffset.UtcNow, "mdaresna-platform",
                IntegrationMessageScope.ForSchool(school.TenantId, school.Id),
                new(school.TenantId.Value, school.Id.Value, school.Code.Value, school.RoutingVersion, target),
                new IntegrationAggregateReference("school-routing", school.Id.Value, school.RoutingVersion));
            OutboxMessages.Add(new PlatformOutboxMessage
            {
                Id = message.MessageId, MessageType = message.MessageType, SchemaVersion = message.SchemaVersion,
                PayloadJson = IntegrationJsonSerializer.Serialize(message), OccurredAtUtc = message.OccurredAtUtc,
                CorrelationId = message.CorrelationId
            });
        }
    }
}
