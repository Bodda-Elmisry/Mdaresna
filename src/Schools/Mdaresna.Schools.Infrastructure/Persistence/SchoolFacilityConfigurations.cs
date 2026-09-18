using Mdaresna.Schools.Domain.Facilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Schools.Infrastructure.Persistence;

internal static class FacilityConfiguration
{
    public static void Common<TEntity>(EntityTypeBuilder<TEntity> b) where TEntity : class, ISoftDeletableSchoolEntity
    {
        b.Property("Code").HasMaxLength(50).IsRequired();
        b.Property("NameAr").HasMaxLength(150).IsRequired();
        b.Property("NameEn").HasMaxLength(150).IsRequired();
        b.HasQueryFilter(x => !x.IsDeleted);
    }
}

internal sealed class SchoolBranchConfiguration : IEntityTypeConfiguration<SchoolBranch>
{
    public void Configure(EntityTypeBuilder<SchoolBranch> b)
    {
        b.ToTable("school_branches"); b.HasKey(x => x.Id); FacilityConfiguration.Common(b);
        b.Property(x => x.Address).HasMaxLength(500);
    }
}

internal sealed class SchoolBuildingConfiguration : IEntityTypeConfiguration<SchoolBuilding>
{
    public void Configure(EntityTypeBuilder<SchoolBuilding> b)
    {
        b.ToTable("school_buildings"); b.HasKey(x => x.Id); FacilityConfiguration.Common(b);
        b.HasOne(x => x.Branch).WithMany(x => x.Buildings).HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class BuildingFloorConfiguration : IEntityTypeConfiguration<BuildingFloor>
{
    public void Configure(EntityTypeBuilder<BuildingFloor> b)
    {
        b.ToTable("building_floors"); b.HasKey(x => x.Id); FacilityConfiguration.Common(b);
        b.HasOne(x => x.Building).WithMany(x => x.Floors).HasForeignKey(x => x.BuildingId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class SchoolRoomTypeConfiguration : IEntityTypeConfiguration<SchoolRoomType>
{
    public void Configure(EntityTypeBuilder<SchoolRoomType> b)
    {
        b.ToTable("school_room_types"); b.HasKey(x => x.Id); FacilityConfiguration.Common(b);
    }
}

internal sealed class RoomCapabilityConfiguration : IEntityTypeConfiguration<RoomCapability>
{
    public void Configure(EntityTypeBuilder<RoomCapability> b)
    {
        b.ToTable("room_capabilities"); b.HasKey(x => x.Id); FacilityConfiguration.Common(b);
    }
}

internal sealed class SchoolRoomConfiguration : IEntityTypeConfiguration<SchoolRoom>
{
    public void Configure(EntityTypeBuilder<SchoolRoom> b)
    {
        b.ToTable("school_rooms"); b.HasKey(x => x.Id); FacilityConfiguration.Common(b);
        b.HasOne(x => x.Floor).WithMany(x => x.Rooms).HasForeignKey(x => x.FloorId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.RoomType).WithMany(x => x.Rooms).HasForeignKey(x => x.RoomTypeId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class SchoolRoomCapabilityConfiguration : IEntityTypeConfiguration<SchoolRoomCapability>
{
    public void Configure(EntityTypeBuilder<SchoolRoomCapability> b)
    {
        b.ToTable("school_room_capabilities"); b.HasKey(x => new { x.RoomId, x.CapabilityId });
        b.HasQueryFilter(x => !x.Room.IsDeleted && !x.Capability.IsDeleted);
        b.HasOne(x => x.Room).WithMany(x => x.Capabilities).HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Capability).WithMany(x => x.Rooms).HasForeignKey(x => x.CapabilityId).OnDelete(DeleteBehavior.Restrict);
    }
}
