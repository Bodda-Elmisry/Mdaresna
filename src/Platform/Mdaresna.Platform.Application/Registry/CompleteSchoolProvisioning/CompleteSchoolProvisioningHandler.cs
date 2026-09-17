using System.Text.Json;
using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.Platform.Application.Abstractions.Messaging;
using Mdaresna.Platform.Application.Abstractions.Persistence;
using Mdaresna.Platform.Application.Abstractions.Security;
using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Application.Registry.Lifecycle;
using Mdaresna.Platform.Contracts.Registry;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Platform.Domain.Registry.Events;
using Mdaresna.Schools.Contracts.Provisioning;
using Mdaresna.SharedKernel.Time;

namespace Mdaresna.Platform.Application.Registry.CompleteSchoolProvisioning;

public sealed class CompleteSchoolProvisioningHandler(
    ISchoolRegistrationRepository schools,
    ISchoolDatabaseEndpointRepository endpoints,
    IPlatformOutboxWriter outbox,
    IPlatformRegistryAuditWriter audit,
    IPlatformUnitOfWork unitOfWork,
    ISharedIdentityAccountContactReader accountContacts,
    IPlatformSmsSender smsSender,
    IClock clock)
{
    public async Task HandleAsync(SchoolProvisionedV1 result, CancellationToken cancellationToken = default)
    {
        var school = await schools.FindByIdAsync(result.SchoolId, cancellationToken)
            ?? throw new PlatformResourceNotFoundException("school.not_found", "Provisioned school was not found.");
        if (school.TenantId != result.TenantId)
            throw new PlatformResourceNotFoundException("school.not_found", "Provisioned school tenant does not match.");
        if (school.Status == SchoolLifecycleStatus.Active && school.ProvisioningOperationId == result.OperationId)
        {
            await SendOwnerReadyAsync(school, result, cancellationToken);
            return;
        }
        if (school.Status != SchoolLifecycleStatus.Provisioning || school.ProvisioningOperationId != result.OperationId)
            throw new PlatformConflictException("school.provisioning_operation_conflict", "Provisioning operation is not current.");
        if (!Enum.TryParse<SchoolDatabaseProvider>(result.Provider, true, out var provider))
            throw new ArgumentException("Unsupported school database provider.", nameof(result));

        var now = result.CompletedAtUtc.Offset == TimeSpan.Zero ? result.CompletedAtUtc : clock.UtcNow;
        var endpoint = await endpoints.FindByIdAsync(result.OperationId, cancellationToken);
        if (endpoint is null)
        {
            endpoint = SchoolDatabaseEndpoint.Create(result.OperationId, school.Id,
                SchoolDatabasePurpose.Operational, provider, result.Host, result.Port,
                result.DatabaseName, result.CredentialSecretReference, result.RequireTls,
                true, null, result.DatabaseSchemaVersion, now);
            await endpoints.AddAsync(endpoint, cancellationToken);
        }
        endpoint.ChangeStatus(SchoolDatabaseEndpointStatus.Active, now);

        var previousCount = school.DomainEvents.Count;
        school.Activate(result.OperationId, school.RequestedByAccountId, now);
        var lifecycle = school.DomainEvents.Skip(previousCount)
            .OfType<SchoolLifecycleChangedDomainEvent>().Single();
        outbox.Enqueue(new IntegrationMessageEnvelope<SchoolLifecycleChangedV1>(
            lifecycle.EventId, SchoolLifecycleChangedV1.MessageType, SchoolLifecycleChangedV1.SchemaVersion,
            lifecycle.OccurredAtUtc, "mdaresna-platform",
            IntegrationMessageScope.ForSchool(school.TenantId, school.Id),
            new IntegrationAggregateReference("school-registration", (Guid)school.Id, school.Version),
            result.OperationId, result.OperationId, null,
            new SchoolLifecycleChangedV1(lifecycle.TenantId, lifecycle.SchoolId,
                lifecycle.PreviousStatus.ToContract(), lifecycle.CurrentStatus.ToContract(),
                lifecycle.ProvisioningOperationId, lifecycle.Reason, lifecycle.OccurredAtUtc)));
        audit.Stage(new RegistryAuditRecord(lifecycle.EventId, IdentityAccountId.From(school.RequestedByAccountId),
            school.TenantId, "platform.school.provisioned", "school-registration", school.Id.ToString(),
            now, result.OperationId, JsonSerializer.Serialize(new
            { result.OperationId, result.LocalSchoolId, endpoint.Id, endpoint.DatabaseName, endpoint.Provider })));
        await unitOfWork.SaveChangesAsync(cancellationToken);
        school.DequeueDomainEvents();
        await SendOwnerReadyAsync(school, result, cancellationToken);
    }

    private async Task SendOwnerReadyAsync(SchoolRegistration school, SchoolProvisionedV1 result,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(result.OwnerFullUserName)) return;
        var phone = await accountContacts.GetPrimaryPhoneAsync(
            IdentityAccountId.From(school.RequestedByAccountId), cancellationToken)
            ?? throw new InvalidOperationException("The school owner primary phone was not found.");
        await smsSender.SendAsync(phone,
            $"تم الانتهاء من إنشاء مدرستك على مدارسنا. اسم الدخول: {result.OwnerFullUserName}. افتح تطبيق المدارس واطلب كود التفعيل لإنشاء كلمة المرور.",
            "school-owner-ready", (Guid)school.Id, cancellationToken);
    }
}
