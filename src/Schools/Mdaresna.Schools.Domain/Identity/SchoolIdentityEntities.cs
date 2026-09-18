namespace Mdaresna.Schools.Domain.Identity;

public enum PersonStatus { Active = 1, Inactive = 2 }
public enum PersonContactType { Phone = 1, Email = 2, Address = 3 }
public enum LocalUserStatus { PendingActivation = 1, Active = 2, Locked = 3, Suspended = 4, Disabled = 5 }
public enum SchoolUserKind { Employee = 1, Teacher = 2 }

public sealed class Person
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? GenderCode { get; set; }
    public PersonStatus Status { get; set; } = PersonStatus.Active;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ICollection<PersonContact> Contacts { get; set; } = [];
    public LocalUserAccount? UserAccount { get; set; }
    public PersonProfileImage? ProfileImage { get; set; }
}

public sealed class PersonProfileImage
{
    public Guid PersonId { get; set; }
    public byte[] Content { get; set; } = [];
    public string ContentType { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public Person Person { get; set; } = null!;
}

public sealed class PersonContact
{
    public Guid Id { get; set; }
    public Guid PersonId { get; set; }
    public PersonContactType Type { get; set; }
    public string Value { get; set; } = string.Empty;
    public string NormalizedValue { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public bool IsVerified { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public Person Person { get; set; } = null!;
}

public sealed class LocalUserAccount
{
    public Guid Id { get; set; }
    public Guid PersonId { get; set; }
    public Guid? PlatformAccountId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string NormalizedUserName { get; set; } = string.Empty;
    public SchoolUserKind Kind { get; set; } = SchoolUserKind.Employee;
    public LocalUserStatus Status { get; set; } = LocalUserStatus.PendingActivation;
    public long PermissionsVersion { get; set; } = 1;
    public DateTimeOffset? LastLoginAtUtc { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public Person Person { get; set; } = null!;
    public LocalUserCredential Credential { get; set; } = null!;
    public ICollection<LocalUserRole> Roles { get; set; } = [];
    public ICollection<LocalUserSession> Sessions { get; set; } = [];
}

public sealed class LocalUserCredential
{
    public Guid UserId { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string SecurityStamp { get; set; } = string.Empty;
    public int FailedSignInCount { get; set; }
    public DateTimeOffset? LockoutEndUtc { get; set; }
    public bool MustChangePassword { get; set; }
    public DateTimeOffset ChangedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public LocalUserAccount User { get; set; } = null!;
}

public sealed class LocalUserActivationChallenge
{
    public Guid UserId { get; set; }
    public byte[] CodeHash { get; set; } = [];
    public byte[] CodeSalt { get; set; } = [];
    public DateTimeOffset ExpiresAtUtc { get; set; }
    public DateTimeOffset? ConsumedAtUtc { get; set; }
    public int FailedAttempts { get; set; }
    public DateTimeOffset? LastSentAtUtc { get; set; }
    public DateTimeOffset? SendWindowStartUtc { get; set; }
    public int SendCount { get; set; }
    public LocalUserAccount User { get; set; } = null!;
}

public sealed class LocalUserSession
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string RefreshTokenHash { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset ExpiresAtUtc { get; set; }
    public DateTimeOffset? RevokedAtUtc { get; set; }
    public string? RevocationReason { get; set; }
    public LocalUserAccount User { get; set; } = null!;
}

public sealed class LocalRole
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string DisplayNameAr { get; set; } = string.Empty;
    public string DisplayNameEn { get; set; } = string.Empty;
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ICollection<LocalRolePermission> Permissions { get; set; } = [];
    public ICollection<LocalUserRole> Users { get; set; } = [];
}

public sealed class LocalPermission
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string DisplayNameAr { get; set; } = string.Empty;
    public string DisplayNameEn { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public ICollection<LocalRolePermission> Roles { get; set; } = [];
}

public sealed class LocalRolePermission
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
    public DateTimeOffset GrantedAtUtc { get; set; }
    public Guid? GrantedByUserId { get; set; }
    public LocalRole Role { get; set; } = null!;
    public LocalPermission Permission { get; set; } = null!;
}

public sealed class LocalUserRole
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTimeOffset AssignedAtUtc { get; set; }
    public Guid? AssignedByUserId { get; set; }
    public LocalUserAccount User { get; set; } = null!;
    public LocalRole Role { get; set; } = null!;
}

public static class SchoolIdentitySeed
{
    public static readonly Guid SchoolAdminRoleId = Guid.Parse("62f4655a-20af-4acc-bb74-c42733e4f713");
    public const string SchoolAdminRoleCode = "school-admin";
    public static readonly DateTimeOffset SeededAtUtc = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public static readonly (Guid Id, string Code, string Module, string Ar, string En)[] Permissions =
    [
        (Guid.Parse("73d1e183-1cad-4fe2-99c5-b39dc7fffd01"), "school.dashboard.view", "dashboard", "عرض لوحة التحكم", "View dashboard"),
        (Guid.Parse("73d1e183-1cad-4fe2-99c5-b39dc7fffd02"), "school.people.view", "people", "عرض الأشخاص", "View people"),
        (Guid.Parse("73d1e183-1cad-4fe2-99c5-b39dc7fffd03"), "school.people.manage", "people", "إدارة الأشخاص", "Manage people"),
        (Guid.Parse("73d1e183-1cad-4fe2-99c5-b39dc7fffd04"), "school.users.view", "users", "عرض المستخدمين", "View users"),
        (Guid.Parse("73d1e183-1cad-4fe2-99c5-b39dc7fffd05"), "school.users.manage", "users", "إدارة المستخدمين", "Manage users"),
        (Guid.Parse("73d1e183-1cad-4fe2-99c5-b39dc7fffd06"), "school.roles.view", "access", "عرض الأدوار", "View roles"),
        (Guid.Parse("73d1e183-1cad-4fe2-99c5-b39dc7fffd07"), "school.roles.manage", "access", "إدارة الأدوار والصلاحيات", "Manage roles and permissions"),
        (Guid.Parse("73d1e183-1cad-4fe2-99c5-b39dc7fffd08"), "school.facilities.view", "facilities", "عرض الهيكلة المادية", "View facilities"),
        (Guid.Parse("73d1e183-1cad-4fe2-99c5-b39dc7fffd09"), "school.facilities.manage", "facilities", "إدارة الهيكلة المادية", "Manage facilities"),
        (Guid.Parse("73d1e183-1cad-4fe2-99c5-b39dc7fffd0a"), "school.facilities.delete", "facilities", "حذف عناصر الهيكلة المادية", "Delete facilities"),
        (Guid.Parse("73d1e183-1cad-4fe2-99c5-b39dc7fffd0b"), "school.facilities.restore", "facilities", "استعادة عناصر الهيكلة المادية", "Restore facilities"),
        (Guid.Parse("73d1e183-1cad-4fe2-99c5-b39dc7fffd0c"), "school.academics.view", "academics", "عرض الهيكلة الأكاديمية", "View academic structure"),
        (Guid.Parse("73d1e183-1cad-4fe2-99c5-b39dc7fffd0d"), "school.academics.manage", "academics", "إدارة الهيكلة الأكاديمية", "Manage academic structure"),
        (Guid.Parse("73d1e183-1cad-4fe2-99c5-b39dc7fffd0e"), "school.academics.delete", "academics", "حذف عناصر الهيكلة الأكاديمية", "Delete academic structure"),
        (Guid.Parse("73d1e183-1cad-4fe2-99c5-b39dc7fffd0f"), "school.academics.restore", "academics", "استعادة عناصر الهيكلة الأكاديمية", "Restore academic structure"),
        (Guid.Parse("73d1e183-1cad-4fe2-99c5-b39dc7fffd10"), "school.operations.view", "operations", "عرض إعدادات التشغيل", "View operations"),
        (Guid.Parse("73d1e183-1cad-4fe2-99c5-b39dc7fffd11"), "school.operations.manage", "operations", "إدارة إعدادات التشغيل", "Manage operations"),
        (Guid.Parse("73d1e183-1cad-4fe2-99c5-b39dc7fffd12"), "school.operations.delete", "operations", "حذف إعدادات التشغيل", "Delete operations"),
        (Guid.Parse("73d1e183-1cad-4fe2-99c5-b39dc7fffd13"), "school.operations.restore", "operations", "استعادة إعدادات التشغيل", "Restore operations"),
        (Guid.Parse("73d1e183-1cad-4fe2-99c5-b39dc7fffd14"), "school.calendar.view", "calendar", "عرض التقويم المدرسي", "View school calendar"),
        (Guid.Parse("73d1e183-1cad-4fe2-99c5-b39dc7fffd15"), "school.calendar.manage", "calendar", "إدارة التقويم المدرسي", "Manage school calendar"),
        (Guid.Parse("73d1e183-1cad-4fe2-99c5-b39dc7fffd16"), "school.calendar.delete", "calendar", "حذف أحداث التقويم", "Delete calendar events"),
        (Guid.Parse("73d1e183-1cad-4fe2-99c5-b39dc7fffd17"), "school.calendar.restore", "calendar", "استعادة أحداث التقويم", "Restore calendar events")
    ];
}
