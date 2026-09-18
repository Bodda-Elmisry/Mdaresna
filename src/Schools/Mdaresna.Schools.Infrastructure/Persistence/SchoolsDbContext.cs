using Microsoft.EntityFrameworkCore;
using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Domain.School;
using Mdaresna.Schools.Domain.Facilities;
using Mdaresna.Schools.Domain.Academics;

namespace Mdaresna.Schools.Infrastructure.Persistence;

/// <summary>
/// Base context for one school's isolated operational database. It is intentionally not registered
/// against a single application-wide connection string; the authenticated school scope will select
/// the database when the persistence routing phase is implemented.
/// </summary>
public class SchoolsDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Person> Persons => Set<Person>();
    public DbSet<PersonContact> PersonContacts => Set<PersonContact>();
    public DbSet<PersonProfileImage> PersonProfileImages => Set<PersonProfileImage>();
    public DbSet<LocalUserAccount> LocalUsers => Set<LocalUserAccount>();
    public DbSet<LocalUserCredential> LocalUserCredentials => Set<LocalUserCredential>();
    public DbSet<LocalUserActivationChallenge> LocalUserActivationChallenges => Set<LocalUserActivationChallenge>();
    public DbSet<LocalUserSession> LocalUserSessions => Set<LocalUserSession>();
    public DbSet<LocalRole> LocalRoles => Set<LocalRole>();
    public DbSet<LocalPermission> LocalPermissions => Set<LocalPermission>();
    public DbSet<LocalRolePermission> LocalRolePermissions => Set<LocalRolePermission>();
    public DbSet<LocalUserRole> LocalUserRoles => Set<LocalUserRole>();
    public DbSet<SchoolInformation> SchoolInformation => Set<SchoolInformation>();
    public DbSet<SchoolBranch> SchoolBranches => Set<SchoolBranch>();
    public DbSet<SchoolBuilding> SchoolBuildings => Set<SchoolBuilding>();
    public DbSet<BuildingFloor> BuildingFloors => Set<BuildingFloor>();
    public DbSet<SchoolRoomType> SchoolRoomTypes => Set<SchoolRoomType>();
    public DbSet<RoomCapability> RoomCapabilities => Set<RoomCapability>();
    public DbSet<SchoolRoom> SchoolRooms => Set<SchoolRoom>();
    public DbSet<SchoolRoomCapability> SchoolRoomCapabilities => Set<SchoolRoomCapability>();
    public DbSet<EducationProgram> EducationPrograms => Set<EducationProgram>();
    public DbSet<AcademicYearDefinition> AcademicYearDefinitions => Set<AcademicYearDefinition>();
    public DbSet<ProgramAcademicYear> ProgramAcademicYears => Set<ProgramAcademicYear>();
    public DbSet<AcademicTerm> AcademicTerms => Set<AcademicTerm>();
    public DbSet<AcademicPeriod> AcademicPeriods => Set<AcademicPeriod>();
    public DbSet<SchoolDaySchedule> SchoolDaySchedules => Set<SchoolDaySchedule>();
    public DbSet<SchoolCalendarEvent> SchoolCalendarEvents => Set<SchoolCalendarEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("school");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SchoolsDbContext).Assembly);
    }
}

public sealed class PostgreSqlSchoolsDbContext(
    DbContextOptions<PostgreSqlSchoolsDbContext> options) : SchoolsDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigurePostgreSqlConcurrency<Person>(modelBuilder, nameof(Person.RowVersion));
        ConfigurePostgreSqlConcurrency<LocalUserAccount>(modelBuilder, nameof(LocalUserAccount.RowVersion));
        ConfigurePostgreSqlConcurrency<LocalUserCredential>(modelBuilder, nameof(LocalUserCredential.RowVersion));
        ConfigurePostgreSqlConcurrency<LocalRole>(modelBuilder, nameof(LocalRole.RowVersion));
        ConfigurePostgreSqlConcurrency<Mdaresna.Schools.Domain.School.SchoolInformation>(
            modelBuilder, nameof(Mdaresna.Schools.Domain.School.SchoolInformation.RowVersion));
        ConfigurePostgreSqlConcurrency<SchoolBranch>(modelBuilder, nameof(SchoolBranch.RowVersion));
        ConfigurePostgreSqlConcurrency<SchoolBuilding>(modelBuilder, nameof(SchoolBuilding.RowVersion));
        ConfigurePostgreSqlConcurrency<BuildingFloor>(modelBuilder, nameof(BuildingFloor.RowVersion));
        ConfigurePostgreSqlConcurrency<SchoolRoomType>(modelBuilder, nameof(SchoolRoomType.RowVersion));
        ConfigurePostgreSqlConcurrency<RoomCapability>(modelBuilder, nameof(RoomCapability.RowVersion));
        ConfigurePostgreSqlConcurrency<SchoolRoom>(modelBuilder, nameof(SchoolRoom.RowVersion));
        ConfigurePostgreSqlConcurrency<EducationProgram>(modelBuilder, nameof(EducationProgram.RowVersion));
        ConfigurePostgreSqlConcurrency<AcademicYearDefinition>(modelBuilder, nameof(AcademicYearDefinition.RowVersion));
        ConfigurePostgreSqlConcurrency<ProgramAcademicYear>(modelBuilder, nameof(ProgramAcademicYear.RowVersion));
        ConfigurePostgreSqlConcurrency<AcademicTerm>(modelBuilder, nameof(AcademicTerm.RowVersion));
        ConfigurePostgreSqlConcurrency<AcademicPeriod>(modelBuilder, nameof(AcademicPeriod.RowVersion));
        ConfigurePostgreSqlConcurrency<SchoolDaySchedule>(modelBuilder, nameof(SchoolDaySchedule.RowVersion));
        ConfigurePostgreSqlConcurrency<SchoolCalendarEvent>(modelBuilder, nameof(SchoolCalendarEvent.RowVersion));
        ConfigureFacilityIndexes(modelBuilder, "\"IsDeleted\" = FALSE");
        ConfigureAcademicIndexes(modelBuilder, "\"IsDeleted\" = FALSE");
    }

    private static void ConfigureFacilityIndexes(ModelBuilder modelBuilder, string filter)
    {
        modelBuilder.Entity<SchoolBranch>().HasIndex(x => x.Code).IsUnique().HasFilter(filter);
        modelBuilder.Entity<SchoolBuilding>().HasIndex(x => new { x.BranchId, x.Code }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<BuildingFloor>().HasIndex(x => new { x.BuildingId, x.Code }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<SchoolRoomType>().HasIndex(x => x.Code).IsUnique().HasFilter(filter);
        modelBuilder.Entity<RoomCapability>().HasIndex(x => x.Code).IsUnique().HasFilter(filter);
        modelBuilder.Entity<SchoolRoom>().HasIndex(x => new { x.FloorId, x.Code }).IsUnique().HasFilter(filter);
    }

    private static void ConfigureAcademicIndexes(ModelBuilder modelBuilder, string filter)
    {
        modelBuilder.Entity<EducationProgram>().HasIndex(x => x.Code).IsUnique().HasFilter(filter);
        modelBuilder.Entity<AcademicYearDefinition>().HasIndex(x => x.Code).IsUnique().HasFilter(filter);
        modelBuilder.Entity<ProgramAcademicYear>().HasIndex(x => new { x.EducationProgramId, x.AcademicYearDefinitionId }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<AcademicTerm>().HasIndex(x => new { x.ProgramAcademicYearId, x.Code }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<AcademicPeriod>().HasIndex(x => new { x.AcademicTermId, x.Code }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<SchoolDaySchedule>().HasIndex(x => new { x.EducationProgramId, x.BranchId, x.DayOfWeek }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<SchoolCalendarEvent>().HasIndex(x => x.Code).IsUnique().HasFilter(filter);
    }

    private static void ConfigurePostgreSqlConcurrency<TEntity>(ModelBuilder modelBuilder, string rowVersionProperty)
        where TEntity : class
    {
        var entity = modelBuilder.Entity<TEntity>();
        entity.Ignore(rowVersionProperty);
        entity.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();
    }
}

public sealed class SqlServerSchoolsDbContext(
    DbContextOptions<SqlServerSchoolsDbContext> options) : SchoolsDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Person>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<LocalUserAccount>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<LocalUserCredential>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<LocalRole>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<Mdaresna.Schools.Domain.School.SchoolInformation>()
            .Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<SchoolBranch>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<SchoolBuilding>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<BuildingFloor>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<SchoolRoomType>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<RoomCapability>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<SchoolRoom>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<EducationProgram>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<AcademicYearDefinition>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<ProgramAcademicYear>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<AcademicTerm>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<AcademicPeriod>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<SchoolDaySchedule>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<SchoolCalendarEvent>().Property(x => x.RowVersion).IsRowVersion();
        ConfigureFacilityIndexes(modelBuilder, "[IsDeleted] = 0");
        ConfigureAcademicIndexes(modelBuilder, "[IsDeleted] = 0");
    }

    private static void ConfigureFacilityIndexes(ModelBuilder modelBuilder, string filter)
    {
        modelBuilder.Entity<SchoolBranch>().HasIndex(x => x.Code).IsUnique().HasFilter(filter);
        modelBuilder.Entity<SchoolBuilding>().HasIndex(x => new { x.BranchId, x.Code }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<BuildingFloor>().HasIndex(x => new { x.BuildingId, x.Code }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<SchoolRoomType>().HasIndex(x => x.Code).IsUnique().HasFilter(filter);
        modelBuilder.Entity<RoomCapability>().HasIndex(x => x.Code).IsUnique().HasFilter(filter);
        modelBuilder.Entity<SchoolRoom>().HasIndex(x => new { x.FloorId, x.Code }).IsUnique().HasFilter(filter);
    }

    private static void ConfigureAcademicIndexes(ModelBuilder modelBuilder, string filter)
    {
        modelBuilder.Entity<EducationProgram>().HasIndex(x => x.Code).IsUnique().HasFilter(filter);
        modelBuilder.Entity<AcademicYearDefinition>().HasIndex(x => x.Code).IsUnique().HasFilter(filter);
        modelBuilder.Entity<ProgramAcademicYear>().HasIndex(x => new { x.EducationProgramId, x.AcademicYearDefinitionId }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<AcademicTerm>().HasIndex(x => new { x.ProgramAcademicYearId, x.Code }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<AcademicPeriod>().HasIndex(x => new { x.AcademicTermId, x.Code }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<SchoolDaySchedule>().HasIndex(x => new { x.EducationProgramId, x.BranchId, x.DayOfWeek }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<SchoolCalendarEvent>().HasIndex(x => x.Code).IsUnique().HasFilter(filter);
    }
}
