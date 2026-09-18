namespace Mdaresna.Schools.Domain.Facilities;

public interface ISoftDeletableSchoolEntity
{
    bool IsDeleted { get; set; }
    DateTimeOffset? DeletedAtUtc { get; set; }
    Guid? DeletedByUserId { get; set; }
}

public sealed class SchoolBranch : ISoftDeletableSchoolEntity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ICollection<SchoolBuilding> Buildings { get; set; } = [];
}

public sealed class SchoolBuilding : ISoftDeletableSchoolEntity
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public SchoolBranch Branch { get; set; } = null!;
    public ICollection<BuildingFloor> Floors { get; set; } = [];
}

public sealed class BuildingFloor : ISoftDeletableSchoolEntity
{
    public Guid Id { get; set; }
    public Guid BuildingId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public SchoolBuilding Building { get; set; } = null!;
    public ICollection<SchoolRoom> Rooms { get; set; } = [];
}

public sealed class SchoolRoomType : ISoftDeletableSchoolEntity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public bool IsLaboratory { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ICollection<SchoolRoom> Rooms { get; set; } = [];
}

public sealed class RoomCapability : ISoftDeletableSchoolEntity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ICollection<SchoolRoomCapability> Rooms { get; set; } = [];
}

public sealed class SchoolRoom : ISoftDeletableSchoolEntity
{
    public Guid Id { get; set; }
    public Guid FloorId { get; set; }
    public Guid RoomTypeId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public bool IsSchedulable { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public BuildingFloor Floor { get; set; } = null!;
    public SchoolRoomType RoomType { get; set; } = null!;
    public ICollection<SchoolRoomCapability> Capabilities { get; set; } = [];
}

public sealed class SchoolRoomCapability
{
    public Guid RoomId { get; set; }
    public Guid CapabilityId { get; set; }
    public SchoolRoom Room { get; set; } = null!;
    public RoomCapability Capability { get; set; } = null!;
}
