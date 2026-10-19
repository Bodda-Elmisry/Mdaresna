using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.Platform.Application.Billing.Subscriptions;
using Mdaresna.Platform.Application.Registry.ConsumeSchoolRegistrationRequest;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;
using Mdaresna.Schools.Contracts.Registration;
using Mdaresna.Tenancy.Abstractions.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Infrastructure.Messaging;

public sealed class PlatformSchoolRegistrationRequestV3Ingestor(PlatformDbContext db, ConsumeSchoolRegistrationRequestV2Handler handler, IPlatformInvoiceService invoices)
{
    private const string Consumer = "platform.school-registration-request.v3";
    public async Task<bool> IngestAsync(IntegrationMessageEnvelope<SchoolRegistrationRequestedV3> envelope, CancellationToken ct = default)
    {
        if (envelope.Producer != "schools" || envelope.SchemaVersion != 3 || envelope.Data.Request == null || envelope.Data.OfferId == Guid.Empty || envelope.Scope.TenantId != envelope.Data.Request.TenantId || envelope.Scope.SchoolId != null)
            throw new ArgumentException("Invalid school subscription registration source.");
        var processed = false;
        await db.Database.CreateExecutionStrategy().ExecuteAsync(async () => {
            db.ChangeTracker.Clear();
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            if (await db.InboxMessages.AnyAsync(x => x.Consumer == Consumer && x.MessageId == envelope.MessageId, ct)) return;
            var request = envelope.Data.Request;
            var legacy = new IntegrationMessageEnvelope<SchoolRegistrationRequestedV2>(envelope.MessageId, SchoolRegistrationRequestedV2.MessageType, 2, envelope.OccurredAtUtc,
                envelope.Producer, envelope.Scope, envelope.Aggregate, envelope.CorrelationId, envelope.CausationId, envelope.TraceParent, request);
            await handler.HandleAsync(legacy, ct);
            var school = await db.Schools.SingleAsync(x => x.Id == SchoolId.From(request.TenantId.Value), ct);
            await invoices.CreateRegistrationInvoiceAsync(envelope.Data.OfferId, request, new(IdentityAccountId.From(school.RequestedByAccountId), envelope.CorrelationId), ct);
            var now = DateTimeOffset.UtcNow;
            db.InboxMessages.Add(new PlatformInboxMessage { Consumer = Consumer, MessageId = envelope.MessageId, MessageType = SchoolRegistrationRequestedV3.MessageType,
                SchemaVersion = 3, ReceivedAtUtc = now, ProcessedAtUtc = now, AttemptCount = 1 });
            await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); processed = true;
        });
        return processed;
    }
}
