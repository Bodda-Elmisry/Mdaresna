using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Domain.Billing;
using Mdaresna.Platform.Domain.Billing.Units;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Billing;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;
using Mdaresna.Platform.Domain.Registry;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform;

public partial class PlatformDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<SchoolRegistration> Schools => Set<SchoolRegistration>();
    public DbSet<SchoolDatabaseEndpoint> SchoolDatabaseEndpoints => Set<SchoolDatabaseEndpoint>();
    public DbSet<GlobalStudentRegistry> GlobalStudents => Set<GlobalStudentRegistry>();
    public DbSet<ExternalIdentifierMapping> ExternalIdentifierMappings => Set<ExternalIdentifierMapping>();
    public DbSet<PlatformRole> Roles => Set<PlatformRole>();
    public DbSet<PlatformPermissionRecord> Permissions => Set<PlatformPermissionRecord>();
    public DbSet<PlatformRolePermissionRecord> RolePermissions => Set<PlatformRolePermissionRecord>();
    public DbSet<PlatformRoleAssignment> RoleAssignments => Set<PlatformRoleAssignment>();
    public DbSet<PlatformLocalUser> LocalUsers => Set<PlatformLocalUser>();
    public DbSet<PlatformLocalCredential> LocalCredentials => Set<PlatformLocalCredential>();
    public DbSet<PlatformLocalPasswordResetChallenge> LocalPasswordResetChallenges =>
        Set<PlatformLocalPasswordResetChallenge>();
    public DbSet<PlatformStaffInvitationChallenge> StaffInvitationChallenges =>
        Set<PlatformStaffInvitationChallenge>();
    public DbSet<PlatformAuditEntry> AuditEntries => Set<PlatformAuditEntry>();
    public DbSet<PlatformFeatureFlag> FeatureFlags => Set<PlatformFeatureFlag>();
    public DbSet<PlatformOutboxMessage> OutboxMessages => Set<PlatformOutboxMessage>();
    public DbSet<PlatformInboxMessage> InboxMessages => Set<PlatformInboxMessage>();
    public DbSet<PlatformPaymentRequest> PlatformPaymentRequests => Set<PlatformPaymentRequest>();
    public DbSet<PlatformPaymentLedgerEntry> PlatformPaymentLedgerEntries => Set<PlatformPaymentLedgerEntry>();
    public DbSet<UnitType> UnitTypes => Set<UnitType>();
    public DbSet<UnitPurchaseIntent> UnitPurchaseIntents => Set<UnitPurchaseIntent>();
    public DbSet<UnitGrant> UnitGrants => Set<UnitGrant>();
    public DbSet<Mdaresna.Platform.Domain.Billing.Subscriptions.SchoolSystemDefinition> SchoolSystems => Set<Mdaresna.Platform.Domain.Billing.Subscriptions.SchoolSystemDefinition>();
    public DbSet<Mdaresna.Platform.Domain.Billing.Subscriptions.SystemPlan> SystemPlans => Set<Mdaresna.Platform.Domain.Billing.Subscriptions.SystemPlan>();
    public DbSet<Mdaresna.Platform.Domain.Billing.Subscriptions.SystemPlanRevision> SystemPlanRevisions => Set<Mdaresna.Platform.Domain.Billing.Subscriptions.SystemPlanRevision>();
    public DbSet<Mdaresna.Platform.Domain.Billing.Subscriptions.SchoolSubscription> SchoolSubscriptions => Set<Mdaresna.Platform.Domain.Billing.Subscriptions.SchoolSubscription>();
    public DbSet<Mdaresna.Platform.Domain.Billing.Subscriptions.SubscriptionQuote> SubscriptionQuotes => Set<Mdaresna.Platform.Domain.Billing.Subscriptions.SubscriptionQuote>();
    public DbSet<PlatformSmsProvider> SmsProviders => Set<PlatformSmsProvider>();
    public DbSet<PlatformSmsLog> SmsLogs => Set<PlatformSmsLog>();
    public DbSet<PlatformUserDevice> UserDevices => Set<PlatformUserDevice>();
    public DbSet<PlatformNotification> Notifications => Set<PlatformNotification>();
    public DbSet<PlatformNotificationRecipient> NotificationRecipients => Set<PlatformNotificationRecipient>();
    public DbSet<PlatformNotificationDelivery> NotificationDeliveries => Set<PlatformNotificationDelivery>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new Configurations.TenantConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.SchoolRegistrationConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.SchoolDatabaseEndpointConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.GlobalStudentRegistryConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.ExternalIdentifierMappingConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.PlatformRoleConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.PlatformPermissionRecordConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.PlatformRolePermissionRecordConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.PlatformRoleAssignmentConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.PlatformLocalUserConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.PlatformLocalCredentialConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.PlatformLocalPasswordResetChallengeConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.PlatformStaffInvitationChallengeConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.PlatformAuditEntryConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.PlatformFeatureFlagConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.PlatformOutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.PlatformInboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new PlatformPaymentRequestConfiguration());
        modelBuilder.ApplyConfiguration(new PlatformPaymentLedgerEntryConfiguration());
        modelBuilder.ApplyConfiguration(new UnitTypeConfiguration());
        modelBuilder.ApplyConfiguration(new UnitPurchaseIntentConfiguration());
        modelBuilder.ApplyConfiguration(new UnitGrantConfiguration());
        SystemCatalogConfiguration.Apply(modelBuilder);
        PlatformInvoiceConfiguration.Apply(modelBuilder);
        modelBuilder.ApplyConfiguration(new Configurations.PlatformSmsProviderConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.PlatformSmsLogConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.PlatformUserDeviceConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.PlatformNotificationConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.PlatformNotificationRecipientConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.PlatformNotificationDeliveryConfiguration());
        if (Database.IsNpgsql())
            PostgreSqlModelAdapter.Apply(modelBuilder);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        EnsureAuditEntriesAreAppendOnly();
        EnsurePaymentLedgerIsAppendOnly();
        EnsureUnitEntitlementsAreAppendOnly();
        PrepareRoutingEventsAsync(false, default).GetAwaiter().GetResult();
        PrepareInvoiceEventsAsync(default).GetAwaiter().GetResult();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        EnsureAuditEntriesAreAppendOnly();
        EnsurePaymentLedgerIsAppendOnly();
        EnsureUnitEntitlementsAreAppendOnly();
        await PrepareRoutingEventsAsync(true, cancellationToken);
        await PrepareInvoiceEventsAsync(cancellationToken);
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void EnsureAuditEntriesAreAppendOnly()
    {
        if (ChangeTracker.Entries<PlatformAuditEntry>()
            .Any(entry => entry.State is EntityState.Modified or EntityState.Deleted))
        {
            throw new InvalidOperationException(
                "Platform audit entries are append-only and cannot be modified or deleted.");
        }
    }

    private void EnsurePaymentLedgerIsAppendOnly()
    {
        if (ChangeTracker.Entries<PlatformPaymentLedgerEntry>()
            .Any(entry => entry.State is EntityState.Modified or EntityState.Deleted))
        {
            throw new InvalidOperationException(
                "Platform payment ledger entries are append-only and cannot be modified or deleted.");
        }
    }

    private void EnsureUnitEntitlementsAreAppendOnly()
    {
        if (ChangeTracker.Entries<Mdaresna.Platform.Domain.Billing.Subscriptions.PlatformInvoiceLine>().Any(x => x.State is EntityState.Modified or EntityState.Deleted) ||
            ChangeTracker.Entries<Mdaresna.Platform.Domain.Billing.Subscriptions.PlatformInvoiceReceipt>().Any(x => x.State is EntityState.Modified or EntityState.Deleted))
            throw new InvalidOperationException("Invoice lines and recorded offline receipts are append-only.");
        foreach (var invoice in ChangeTracker.Entries<Mdaresna.Platform.Domain.Billing.Subscriptions.PlatformInvoice>().Where(x => x.State == EntityState.Modified))
            if (new[] { "Number", "SchoolId", "RegistrationRequestId", "Amount", "Currency", "CreatedAtUtc" }.Any(name => invoice.Property(name).IsModified))
                throw new InvalidOperationException("Issued invoice terms are immutable.");
        if (ChangeTracker.Entries<Mdaresna.Platform.Domain.Billing.Subscriptions.SystemPlanRevision>()
                .Any(entry => entry.State is EntityState.Modified or EntityState.Deleted))
            throw new InvalidOperationException("System plan revisions are append-only.");
        if (ChangeTracker.Entries<UnitPurchaseIntent>()
                .Any(entry => entry.State is EntityState.Modified or EntityState.Deleted) ||
            ChangeTracker.Entries<UnitGrant>()
                .Any(entry => entry.State is EntityState.Modified or EntityState.Deleted))
        {
            throw new InvalidOperationException(
                "Unit purchase intents and grants are append-only and cannot be modified or deleted.");
        }
    }
}
