using Mdaresna.Platform.Infrastructure.Persistence.Identity.Entities;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Infrastructure.Persistence.Identity;

public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<LoginIdentifier> LoginIdentifiers => Set<LoginIdentifier>();
    public DbSet<PasswordCredential> PasswordCredentials => Set<PasswordCredential>();
    public DbSet<IdentitySession> Sessions => Set<IdentitySession>();
    public DbSet<MfaMethod> MfaMethods => Set<MfaMethod>();
    public DbSet<IdentitySecurityEvent> SecurityEvents => Set<IdentitySecurityEvent>();
    public DbSet<IdentityOutboxMessage> OutboxMessages => Set<IdentityOutboxMessage>();
    public DbSet<AccountActivationChallenge> ActivationChallenges => Set<AccountActivationChallenge>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new Configurations.AccountConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.LoginIdentifierConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.PasswordCredentialConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.IdentitySessionConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.MfaMethodConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.IdentitySecurityEventConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.IdentityOutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.AccountActivationChallengeConfiguration());
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        EnsureSecurityEventsAreAppendOnly();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        EnsureSecurityEventsAreAppendOnly();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void EnsureSecurityEventsAreAppendOnly()
    {
        if (ChangeTracker.Entries<IdentitySecurityEvent>()
            .Any(entry => entry.State is EntityState.Modified or EntityState.Deleted))
        {
            throw new InvalidOperationException(
                "Identity security events are append-only and cannot be modified or deleted.");
        }
    }
}
