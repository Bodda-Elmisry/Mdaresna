using Mdaresna.Schools.Domain.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Schools.Infrastructure.Persistence;

internal sealed class SchoolSubscriptionStateConfiguration : IEntityTypeConfiguration<SchoolSubscriptionState>
{
    public void Configure(EntityTypeBuilder<SchoolSubscriptionState> b)
    {
        b.ToTable("subscription_states"); b.HasKey(x => x.SystemId); b.Property(x => x.SystemId).ValueGeneratedNever();
        b.Property(x => x.SystemCode).HasMaxLength(32).IsRequired(); b.Property(x => x.CapacityMetric).HasMaxLength(24).IsRequired();
        b.Property(x => x.CapabilityCodes).HasMaxLength(4000).IsRequired(); b.Property(x => x.LocalVersion).IsConcurrencyToken();
        b.HasIndex(x => x.SubscriptionId).IsUnique();
    }
}
internal sealed class SchoolCapacityAllocationConfiguration : IEntityTypeConfiguration<SchoolCapacityAllocation>
{
    public void Configure(EntityTypeBuilder<SchoolCapacityAllocation> b)
    {
        b.ToTable("capacity_allocations"); b.HasKey(x => x.Id); b.HasIndex(x => new { x.SystemId, x.SubjectId }).IsUnique();
        b.HasOne<SchoolSubscriptionState>().WithMany().HasForeignKey(x => x.SystemId).OnDelete(DeleteBehavior.Restrict);
    }
}
internal sealed class SchoolCapacityMovementConfiguration : IEntityTypeConfiguration<SchoolCapacityMovement>
{
    public void Configure(EntityTypeBuilder<SchoolCapacityMovement> b)
    {
        b.ToTable("capacity_movements"); b.HasKey(x => x.Id); b.Property(x => x.Action).HasMaxLength(64).IsRequired();
        b.HasIndex(x => new { x.SystemId, x.OccurredAtUtc });
        b.HasOne<SchoolSubscriptionState>().WithMany().HasForeignKey(x => x.SystemId).OnDelete(DeleteBehavior.Restrict);
    }
}
