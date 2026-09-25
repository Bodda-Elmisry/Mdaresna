using Microsoft.EntityFrameworkCore;
using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Domain.School;
using Mdaresna.Schools.Domain.Facilities;
using Mdaresna.Schools.Domain.Academics;
using Mdaresna.Schools.Domain.Organization;
using Mdaresna.Schools.Domain.Students;

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
    public DbSet<StaffAbsence> StaffAbsences => Set<StaffAbsence>();
    public DbSet<SchoolUserNotification> SchoolUserNotifications => Set<SchoolUserNotification>();
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
    public DbSet<EducationStage> EducationStages => Set<EducationStage>();
    public DbSet<EducationTrack> EducationTracks => Set<EducationTrack>();
    public DbSet<GradeLevel> GradeLevels => Set<GradeLevel>();
    public DbSet<GradeOffering> GradeOfferings => Set<GradeOffering>();
    public DbSet<ClassSection> ClassSections => Set<ClassSection>();
    public DbSet<ClassRoomAssignment> ClassRoomAssignments => Set<ClassRoomAssignment>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<CurriculumPlan> CurriculumPlans => Set<CurriculumPlan>();
    public DbSet<CurriculumGradeSubject> CurriculumGradeSubjects => Set<CurriculumGradeSubject>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<BookVersion> BookVersions => Set<BookVersion>();
    public DbSet<BookRole> BookRoles => Set<BookRole>();
    public DbSet<CurriculumSubjectBook> CurriculumSubjectBooks => Set<CurriculumSubjectBook>();
    public DbSet<GradeSubjectOffering> GradeSubjectOfferings => Set<GradeSubjectOffering>();
    public DbSet<TeacherGradeSubjectScope> TeacherGradeSubjectScopes => Set<TeacherGradeSubjectScope>();
    public DbSet<ClassSectionTeacherScope> ClassSectionTeacherScopes => Set<ClassSectionTeacherScope>();
    public DbSet<ClassSectionSubject> ClassSectionSubjects => Set<ClassSectionSubject>();
    public DbSet<ClassSubjectTeacherAssignment> ClassSubjectTeacherAssignments => Set<ClassSubjectTeacherAssignment>();
    public DbSet<WeeklyTimetableSlot> WeeklyTimetableSlots => Set<WeeklyTimetableSlot>();
    public DbSet<WeeklyTimetableSlotSubstituteTeacher> WeeklyTimetableSlotSubstituteTeachers => Set<WeeklyTimetableSlotSubstituteTeacher>();
    public DbSet<TemporaryClassMerge> TemporaryClassMerges => Set<TemporaryClassMerge>();
    public DbSet<TemporaryClassMergeSection> TemporaryClassMergeSections => Set<TemporaryClassMergeSection>();
    public DbSet<TeacherSubstitution> TeacherSubstitutions => Set<TeacherSubstitution>();
    public DbSet<SchoolDepartment> SchoolDepartments => Set<SchoolDepartment>();
    public DbSet<DepartmentMembership> DepartmentMemberships => Set<DepartmentMembership>();
    public DbSet<DepartmentLeadership> DepartmentLeaderships => Set<DepartmentLeadership>();
    public DbSet<AcademicDepartmentSubject> AcademicDepartmentSubjects => Set<AcademicDepartmentSubject>();
    public DbSet<SubjectCoordinatorAssignment> SubjectCoordinatorAssignments => Set<SubjectCoordinatorAssignment>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Guardian> Guardians => Set<Guardian>();
    public DbSet<StudentGuardian> StudentGuardians => Set<StudentGuardian>();
    public DbSet<StudentEnrollment> StudentEnrollments => Set<StudentEnrollment>();
    public DbSet<AdmissionApplication> AdmissionApplications => Set<AdmissionApplication>();
    public DbSet<AdmissionApplicationGuardian> AdmissionApplicationGuardians => Set<AdmissionApplicationGuardian>();
    public DbSet<StudentAttendanceRegister> StudentAttendanceRegisters => Set<StudentAttendanceRegister>();
    public DbSet<StudentAttendanceEntry> StudentAttendanceEntries => Set<StudentAttendanceEntry>();
    public DbSet<StudentAttendanceAudit> StudentAttendanceAudits => Set<StudentAttendanceAudit>();

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
        ConfigurePostgreSqlConcurrency<EducationStage>(modelBuilder, nameof(EducationStage.RowVersion));
        ConfigurePostgreSqlConcurrency<EducationTrack>(modelBuilder, nameof(EducationTrack.RowVersion));
        ConfigurePostgreSqlConcurrency<GradeLevel>(modelBuilder, nameof(GradeLevel.RowVersion));
        ConfigurePostgreSqlConcurrency<GradeOffering>(modelBuilder, nameof(GradeOffering.RowVersion));
        ConfigurePostgreSqlConcurrency<ClassSection>(modelBuilder, nameof(ClassSection.RowVersion));
        ConfigurePostgreSqlConcurrency<ClassRoomAssignment>(modelBuilder, nameof(ClassRoomAssignment.RowVersion));
        ConfigurePostgreSqlConcurrency<Subject>(modelBuilder, nameof(Subject.RowVersion));
        ConfigurePostgreSqlConcurrency<CurriculumPlan>(modelBuilder, nameof(CurriculumPlan.RowVersion));
        ConfigurePostgreSqlConcurrency<CurriculumGradeSubject>(modelBuilder, nameof(CurriculumGradeSubject.RowVersion));
        ConfigurePostgreSqlConcurrency<Book>(modelBuilder, nameof(Book.RowVersion));
        ConfigurePostgreSqlConcurrency<BookVersion>(modelBuilder, nameof(BookVersion.RowVersion));
        ConfigurePostgreSqlConcurrency<BookRole>(modelBuilder, nameof(BookRole.RowVersion));
        ConfigurePostgreSqlConcurrency<CurriculumSubjectBook>(modelBuilder, nameof(CurriculumSubjectBook.RowVersion));
        ConfigurePostgreSqlConcurrency<GradeSubjectOffering>(modelBuilder, nameof(GradeSubjectOffering.RowVersion));
        ConfigurePostgreSqlConcurrency<TeacherGradeSubjectScope>(modelBuilder, nameof(TeacherGradeSubjectScope.RowVersion));
        ConfigurePostgreSqlConcurrency<ClassSectionTeacherScope>(modelBuilder, nameof(ClassSectionTeacherScope.RowVersion));
        ConfigurePostgreSqlConcurrency<ClassSectionSubject>(modelBuilder, nameof(ClassSectionSubject.RowVersion));
        ConfigurePostgreSqlConcurrency<ClassSubjectTeacherAssignment>(modelBuilder, nameof(ClassSubjectTeacherAssignment.RowVersion));
        ConfigurePostgreSqlConcurrency<WeeklyTimetableSlot>(modelBuilder, nameof(WeeklyTimetableSlot.RowVersion));
        ConfigurePostgreSqlConcurrency<WeeklyTimetableSlotSubstituteTeacher>(modelBuilder, nameof(WeeklyTimetableSlotSubstituteTeacher.RowVersion));
        ConfigurePostgreSqlConcurrency<TemporaryClassMerge>(modelBuilder, nameof(TemporaryClassMerge.RowVersion));
        ConfigurePostgreSqlConcurrency<TemporaryClassMergeSection>(modelBuilder, nameof(TemporaryClassMergeSection.RowVersion));
        ConfigurePostgreSqlConcurrency<TeacherSubstitution>(modelBuilder, nameof(TeacherSubstitution.RowVersion));
        ConfigurePostgreSqlConcurrency<StaffAbsence>(modelBuilder, nameof(StaffAbsence.RowVersion));
        ConfigurePostgreSqlConcurrency<SchoolUserNotification>(modelBuilder, nameof(SchoolUserNotification.RowVersion));
        ConfigurePostgreSqlConcurrency<SchoolDepartment>(modelBuilder, nameof(SchoolDepartment.RowVersion));
        ConfigurePostgreSqlConcurrency<DepartmentMembership>(modelBuilder, nameof(DepartmentMembership.RowVersion));
        ConfigurePostgreSqlConcurrency<DepartmentLeadership>(modelBuilder, nameof(DepartmentLeadership.RowVersion));
        ConfigurePostgreSqlConcurrency<AcademicDepartmentSubject>(modelBuilder, nameof(AcademicDepartmentSubject.RowVersion));
        ConfigurePostgreSqlConcurrency<SubjectCoordinatorAssignment>(modelBuilder, nameof(SubjectCoordinatorAssignment.RowVersion));
        ConfigurePostgreSqlConcurrency<Student>(modelBuilder, nameof(Student.RowVersion));
        ConfigurePostgreSqlConcurrency<Guardian>(modelBuilder, nameof(Guardian.RowVersion));
        ConfigurePostgreSqlConcurrency<StudentGuardian>(modelBuilder, nameof(StudentGuardian.RowVersion));
        ConfigurePostgreSqlConcurrency<StudentEnrollment>(modelBuilder, nameof(StudentEnrollment.RowVersion));
        ConfigurePostgreSqlConcurrency<AdmissionApplication>(modelBuilder, nameof(AdmissionApplication.RowVersion));
        ConfigurePostgreSqlConcurrency<StudentAttendanceRegister>(modelBuilder, nameof(StudentAttendanceRegister.RowVersion));
        ConfigurePostgreSqlConcurrency<StudentAttendanceEntry>(modelBuilder, nameof(StudentAttendanceEntry.RowVersion));
        ConfigureFacilityIndexes(modelBuilder, "\"IsDeleted\" = FALSE");
        ConfigureAcademicIndexes(modelBuilder, "\"IsDeleted\" = FALSE");
        ConfigureOrganizationIndexes(modelBuilder, "\"IsDeleted\" = FALSE", "\"IsDeleted\" = FALSE AND \"IsPrimary\" = TRUE");
        modelBuilder.Entity<StaffAbsence>().HasIndex(x => new { x.UserId, x.StartsOn, x.EndsOn }).HasFilter("\"IsDeleted\" = FALSE");
        modelBuilder.Entity<StudentGuardian>().HasIndex(x => x.StudentId).IsUnique().HasFilter("\"IsActive\" = TRUE AND \"IsPrimary\" = TRUE");
        modelBuilder.Entity<AdmissionApplicationGuardian>().HasIndex(x => x.AdmissionApplicationId).IsUnique().HasFilter("\"IsPrimary\" = TRUE");
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
        modelBuilder.Entity<EducationStage>().HasIndex(x => new { x.EducationProgramId, x.Code }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<EducationTrack>().HasIndex(x => new { x.EducationStageId, x.Code }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<GradeLevel>().HasIndex(x => new { x.EducationStageId, x.Code }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<GradeOffering>().HasIndex(x => new { x.ProgramAcademicYearId, x.GradeLevelId }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<ClassSection>().HasIndex(x => new { x.GradeOfferingId, x.Code }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<ClassRoomAssignment>().HasIndex(x => new { x.ClassSectionId, x.RoomId, x.EffectiveFrom, x.EffectiveTo }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<Subject>().HasIndex(x => x.Code).IsUnique().HasFilter(filter);
        modelBuilder.Entity<CurriculumPlan>().HasIndex(x => new { x.EducationProgramId, x.Code, x.VersionLabel }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<CurriculumGradeSubject>().HasIndex(x => new { x.CurriculumPlanId, x.GradeLevelId, x.SubjectId, x.TermNumber }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<Book>().HasIndex(x => x.Code).IsUnique().HasFilter(filter);
        modelBuilder.Entity<BookVersion>().HasIndex(x => new { x.BookId, x.EditionCode }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<BookRole>().HasIndex(x => x.Code).IsUnique().HasFilter(filter);
        modelBuilder.Entity<CurriculumSubjectBook>().HasIndex(x => new { x.CurriculumGradeSubjectId, x.BookVersionId, x.BookRoleId }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<GradeSubjectOffering>().HasIndex(x => new { x.GradeOfferingId, x.CurriculumGradeSubjectId }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<TeacherGradeSubjectScope>().HasIndex(x => new { x.TeacherUserId, x.GradeSubjectOfferingId }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<ClassSectionTeacherScope>().HasIndex(x => new { x.ClassSectionId, x.TeacherGradeSubjectScopeId }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<ClassSectionSubject>().HasIndex(x => new { x.ClassSectionId, x.GradeSubjectOfferingId }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<ClassSubjectTeacherAssignment>().HasIndex(x => new { x.ClassSectionSubjectId, x.Role, x.TeacherGradeSubjectScopeId }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<WeeklyTimetableSlot>().HasIndex(x => new { x.ClassSectionId, x.DayOfWeek, x.SlotNumber }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<WeeklyTimetableSlotSubstituteTeacher>().HasIndex(x => new { x.WeeklyTimetableSlotId, x.TeacherGradeSubjectScopeId }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<TemporaryClassMerge>().HasIndex(x => new { x.RoomId, x.MergeDate, x.StartsAt, x.EndsAt }).HasFilter(filter);
        modelBuilder.Entity<TemporaryClassMergeSection>().HasIndex(x => new { x.TemporaryClassMergeId, x.ClassSectionId }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<TeacherSubstitution>().HasIndex(x => new { x.WeeklyTimetableSlotId, x.LessonDate }).IsUnique().HasFilter(filter);
    }

    private static void ConfigureOrganizationIndexes(ModelBuilder modelBuilder, string filter, string primaryFilter)
    {
        modelBuilder.Entity<SchoolDepartment>().HasIndex(x => x.Code).IsUnique().HasFilter(filter);
        modelBuilder.Entity<DepartmentMembership>().HasIndex(x => new { x.DepartmentId, x.UserId }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<DepartmentMembership>().HasIndex(x => x.UserId).IsUnique().HasFilter(primaryFilter);
        modelBuilder.Entity<DepartmentLeadership>().HasIndex(x => new { x.DepartmentId, x.Role }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<AcademicDepartmentSubject>().HasIndex(x => new { x.DepartmentId, x.SubjectId }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<SubjectCoordinatorAssignment>().HasIndex(x => new { x.DepartmentSubjectId, x.EducationProgramId, x.EducationStageId }).IsUnique().HasFilter(filter);
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
        modelBuilder.Entity<EducationStage>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<EducationTrack>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<GradeLevel>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<GradeOffering>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<ClassSection>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<ClassRoomAssignment>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<Subject>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<CurriculumPlan>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<CurriculumGradeSubject>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<Book>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<BookVersion>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<BookRole>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<CurriculumSubjectBook>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<GradeSubjectOffering>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<TeacherGradeSubjectScope>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<ClassSectionTeacherScope>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<ClassSectionSubject>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<ClassSubjectTeacherAssignment>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<WeeklyTimetableSlot>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<WeeklyTimetableSlotSubstituteTeacher>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<TemporaryClassMerge>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<TemporaryClassMergeSection>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<TeacherSubstitution>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<StaffAbsence>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<SchoolUserNotification>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<SchoolDepartment>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<DepartmentMembership>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<DepartmentLeadership>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<AcademicDepartmentSubject>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<SubjectCoordinatorAssignment>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<Student>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<Guardian>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<StudentGuardian>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<StudentEnrollment>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<AdmissionApplication>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<StudentAttendanceRegister>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<StudentAttendanceEntry>().Property(x => x.RowVersion).IsRowVersion();
        ConfigureFacilityIndexes(modelBuilder, "[IsDeleted] = 0");
        ConfigureAcademicIndexes(modelBuilder, "[IsDeleted] = 0");
        ConfigureOrganizationIndexes(modelBuilder, "[IsDeleted] = 0", "[IsDeleted] = 0 AND [IsPrimary] = 1");
        modelBuilder.Entity<StudentGuardian>().HasIndex(x => x.StudentId).IsUnique().HasFilter("[IsActive] = 1 AND [IsPrimary] = 1");
        modelBuilder.Entity<AdmissionApplicationGuardian>().HasIndex(x => x.AdmissionApplicationId).IsUnique().HasFilter("[IsPrimary] = 1");
        modelBuilder.Entity<StaffAbsence>().HasIndex(x => new { x.UserId, x.StartsOn, x.EndsOn }).HasFilter("[IsDeleted] = 0");
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
        modelBuilder.Entity<EducationStage>().HasIndex(x => new { x.EducationProgramId, x.Code }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<EducationTrack>().HasIndex(x => new { x.EducationStageId, x.Code }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<GradeLevel>().HasIndex(x => new { x.EducationStageId, x.Code }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<GradeOffering>().HasIndex(x => new { x.ProgramAcademicYearId, x.GradeLevelId }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<ClassSection>().HasIndex(x => new { x.GradeOfferingId, x.Code }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<ClassRoomAssignment>().HasIndex(x => new { x.ClassSectionId, x.RoomId, x.EffectiveFrom, x.EffectiveTo }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<Subject>().HasIndex(x => x.Code).IsUnique().HasFilter(filter);
        modelBuilder.Entity<CurriculumPlan>().HasIndex(x => new { x.EducationProgramId, x.Code, x.VersionLabel }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<CurriculumGradeSubject>().HasIndex(x => new { x.CurriculumPlanId, x.GradeLevelId, x.SubjectId, x.TermNumber }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<Book>().HasIndex(x => x.Code).IsUnique().HasFilter(filter);
        modelBuilder.Entity<BookVersion>().HasIndex(x => new { x.BookId, x.EditionCode }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<BookRole>().HasIndex(x => x.Code).IsUnique().HasFilter(filter);
        modelBuilder.Entity<CurriculumSubjectBook>().HasIndex(x => new { x.CurriculumGradeSubjectId, x.BookVersionId, x.BookRoleId }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<GradeSubjectOffering>().HasIndex(x => new { x.GradeOfferingId, x.CurriculumGradeSubjectId }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<TeacherGradeSubjectScope>().HasIndex(x => new { x.TeacherUserId, x.GradeSubjectOfferingId }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<ClassSectionTeacherScope>().HasIndex(x => new { x.ClassSectionId, x.TeacherGradeSubjectScopeId }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<ClassSectionSubject>().HasIndex(x => new { x.ClassSectionId, x.GradeSubjectOfferingId }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<ClassSubjectTeacherAssignment>().HasIndex(x => new { x.ClassSectionSubjectId, x.Role, x.TeacherGradeSubjectScopeId }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<WeeklyTimetableSlot>().HasIndex(x => new { x.ClassSectionId, x.DayOfWeek, x.SlotNumber }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<WeeklyTimetableSlotSubstituteTeacher>().HasIndex(x => new { x.WeeklyTimetableSlotId, x.TeacherGradeSubjectScopeId }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<TemporaryClassMerge>().HasIndex(x => new { x.RoomId, x.MergeDate, x.StartsAt, x.EndsAt }).HasFilter(filter);
        modelBuilder.Entity<TemporaryClassMergeSection>().HasIndex(x => new { x.TemporaryClassMergeId, x.ClassSectionId }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<TeacherSubstitution>().HasIndex(x => new { x.WeeklyTimetableSlotId, x.LessonDate }).IsUnique().HasFilter(filter);
    }

    private static void ConfigureOrganizationIndexes(ModelBuilder modelBuilder, string filter, string primaryFilter)
    {
        modelBuilder.Entity<SchoolDepartment>().HasIndex(x => x.Code).IsUnique().HasFilter(filter);
        modelBuilder.Entity<DepartmentMembership>().HasIndex(x => new { x.DepartmentId, x.UserId }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<DepartmentMembership>().HasIndex(x => x.UserId).IsUnique().HasFilter(primaryFilter);
        modelBuilder.Entity<DepartmentLeadership>().HasIndex(x => new { x.DepartmentId, x.Role }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<AcademicDepartmentSubject>().HasIndex(x => new { x.DepartmentId, x.SubjectId }).IsUnique().HasFilter(filter);
        modelBuilder.Entity<SubjectCoordinatorAssignment>().HasIndex(x => new { x.DepartmentSubjectId, x.EducationProgramId, x.EducationStageId }).IsUnique().HasFilter(filter);
    }
}
