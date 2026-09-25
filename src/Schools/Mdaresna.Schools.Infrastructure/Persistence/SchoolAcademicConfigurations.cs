using Mdaresna.Schools.Domain.Academics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Schools.Infrastructure.Persistence;

internal static class AcademicConfiguration
{
    public static void Common<TEntity>(EntityTypeBuilder<TEntity> b) where TEntity : class, Mdaresna.Schools.Domain.Facilities.ISoftDeletableSchoolEntity
    {
        b.HasQueryFilter(x => !x.IsDeleted);
    }
    public static void Named<TEntity>(EntityTypeBuilder<TEntity> b) where TEntity : class, Mdaresna.Schools.Domain.Facilities.ISoftDeletableSchoolEntity
    {
        Common(b); b.Property("Code").HasMaxLength(50).IsRequired();
        b.Property("NameAr").HasMaxLength(150).IsRequired(); b.Property("NameEn").HasMaxLength(150).IsRequired();
    }
}

internal sealed class EducationProgramConfiguration : IEntityTypeConfiguration<EducationProgram>
{
    public void Configure(EntityTypeBuilder<EducationProgram> b) { b.ToTable("education_programs"); b.HasKey(x => x.Id); AcademicConfiguration.Named(b); b.Property(x => x.ProgramType).HasConversion<string>().HasMaxLength(40); }
}
internal sealed class AcademicYearDefinitionConfiguration : IEntityTypeConfiguration<AcademicYearDefinition>
{
    public void Configure(EntityTypeBuilder<AcademicYearDefinition> b) { b.ToTable("academic_year_definitions"); b.HasKey(x => x.Id); AcademicConfiguration.Named(b); }
}
internal sealed class ProgramAcademicYearConfiguration : IEntityTypeConfiguration<ProgramAcademicYear>
{
    public void Configure(EntityTypeBuilder<ProgramAcademicYear> b) { b.ToTable("program_academic_years"); b.HasKey(x => x.Id); AcademicConfiguration.Named(b); b.HasOne(x => x.EducationProgram).WithMany(x => x.AcademicYears).HasForeignKey(x => x.EducationProgramId).OnDelete(DeleteBehavior.Restrict); b.HasOne(x => x.AcademicYearDefinition).WithMany(x => x.ProgramYears).HasForeignKey(x => x.AcademicYearDefinitionId).OnDelete(DeleteBehavior.Restrict); b.HasOne(x => x.CurriculumPlan).WithMany(x => x.ProgramYears).HasForeignKey(x => x.CurriculumPlanId).OnDelete(DeleteBehavior.Restrict); }
}
internal sealed class AcademicTermConfiguration : IEntityTypeConfiguration<AcademicTerm>
{
    public void Configure(EntityTypeBuilder<AcademicTerm> b) { b.ToTable("academic_terms"); b.HasKey(x => x.Id); AcademicConfiguration.Named(b); b.HasOne(x => x.ProgramAcademicYear).WithMany(x => x.Terms).HasForeignKey(x => x.ProgramAcademicYearId).OnDelete(DeleteBehavior.Restrict); }
}
internal sealed class AcademicPeriodConfiguration : IEntityTypeConfiguration<AcademicPeriod>
{
    public void Configure(EntityTypeBuilder<AcademicPeriod> b) { b.ToTable("academic_periods"); b.HasKey(x => x.Id); AcademicConfiguration.Named(b); b.HasOne(x => x.AcademicTerm).WithMany(x => x.Periods).HasForeignKey(x => x.AcademicTermId).OnDelete(DeleteBehavior.Restrict); }
}
internal sealed class SchoolDayScheduleConfiguration : IEntityTypeConfiguration<SchoolDaySchedule>
{
    public void Configure(EntityTypeBuilder<SchoolDaySchedule> b) { b.ToTable("school_day_schedules"); b.HasKey(x => x.Id); AcademicConfiguration.Common(b); b.Property(x => x.DayOfWeek).HasConversion<string>().HasMaxLength(16); b.HasOne(x => x.EducationProgram).WithMany(x => x.Schedules).HasForeignKey(x => x.EducationProgramId).OnDelete(DeleteBehavior.Restrict); b.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict); }
}
internal sealed class SchoolCalendarEventConfiguration : IEntityTypeConfiguration<SchoolCalendarEvent>
{
    public void Configure(EntityTypeBuilder<SchoolCalendarEvent> b) { b.ToTable("school_calendar_events"); b.HasKey(x => x.Id); AcademicConfiguration.Named(b); b.Property(x => x.EventType).HasConversion<string>().HasMaxLength(32); b.HasOne(x => x.EducationProgram).WithMany().HasForeignKey(x => x.EducationProgramId).OnDelete(DeleteBehavior.Restrict); b.HasOne(x => x.ProgramAcademicYear).WithMany().HasForeignKey(x => x.ProgramAcademicYearId).OnDelete(DeleteBehavior.Restrict); b.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict); }
}
internal sealed class EducationStageConfiguration : IEntityTypeConfiguration<EducationStage>
{
    public void Configure(EntityTypeBuilder<EducationStage> b) { b.ToTable("education_stages"); b.HasKey(x => x.Id); AcademicConfiguration.Named(b); b.HasOne(x => x.EducationProgram).WithMany(x => x.Stages).HasForeignKey(x => x.EducationProgramId).OnDelete(DeleteBehavior.Restrict); }
}
internal sealed class EducationTrackConfiguration : IEntityTypeConfiguration<EducationTrack>
{
    public void Configure(EntityTypeBuilder<EducationTrack> b) { b.ToTable("education_tracks"); b.HasKey(x => x.Id); AcademicConfiguration.Named(b); b.HasOne(x => x.EducationStage).WithMany(x => x.Tracks).HasForeignKey(x => x.EducationStageId).OnDelete(DeleteBehavior.Restrict); }
}
internal sealed class GradeLevelConfiguration : IEntityTypeConfiguration<GradeLevel>
{
    public void Configure(EntityTypeBuilder<GradeLevel> b) { b.ToTable("grade_levels"); b.HasKey(x => x.Id); AcademicConfiguration.Named(b); b.HasOne(x => x.EducationStage).WithMany(x => x.GradeLevels).HasForeignKey(x => x.EducationStageId).OnDelete(DeleteBehavior.Restrict); b.HasOne(x => x.EducationTrack).WithMany(x => x.GradeLevels).HasForeignKey(x => x.EducationTrackId).OnDelete(DeleteBehavior.Restrict); }
}
internal sealed class GradeOfferingConfiguration : IEntityTypeConfiguration<GradeOffering>
{
    public void Configure(EntityTypeBuilder<GradeOffering> b) { b.ToTable("grade_offerings"); b.HasKey(x => x.Id); AcademicConfiguration.Named(b); b.Property(x => x.Status).HasConversion<string>().HasMaxLength(24); b.HasOne(x => x.ProgramAcademicYear).WithMany(x => x.GradeOfferings).HasForeignKey(x => x.ProgramAcademicYearId).OnDelete(DeleteBehavior.Restrict); b.HasOne(x => x.GradeLevel).WithMany(x => x.Offerings).HasForeignKey(x => x.GradeLevelId).OnDelete(DeleteBehavior.Restrict); }
}
internal sealed class ClassSectionConfiguration : IEntityTypeConfiguration<ClassSection>
{
    public void Configure(EntityTypeBuilder<ClassSection> b) { b.ToTable("class_sections"); b.HasKey(x => x.Id); AcademicConfiguration.Named(b); b.Property(x => x.Shift).HasConversion<string>().HasMaxLength(24); b.HasOne(x => x.GradeOffering).WithMany(x => x.ClassSections).HasForeignKey(x => x.GradeOfferingId).OnDelete(DeleteBehavior.Restrict); }
}
internal sealed class ClassRoomAssignmentConfiguration : IEntityTypeConfiguration<ClassRoomAssignment>
{
    public void Configure(EntityTypeBuilder<ClassRoomAssignment> b) { b.ToTable("class_room_assignments"); b.HasKey(x => x.Id); AcademicConfiguration.Common(b); b.HasOne(x => x.ClassSection).WithMany(x => x.RoomAssignments).HasForeignKey(x => x.ClassSectionId).OnDelete(DeleteBehavior.Restrict); b.HasOne(x => x.Room).WithMany(x => x.ClassAssignments).HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Restrict); }
}

internal sealed class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> b) { b.ToTable("subjects"); b.HasKey(x => x.Id); AcademicConfiguration.Named(b); }
}
internal sealed class CurriculumPlanConfiguration : IEntityTypeConfiguration<CurriculumPlan>
{
    public void Configure(EntityTypeBuilder<CurriculumPlan> b) { b.ToTable("curriculum_plans"); b.HasKey(x => x.Id); AcademicConfiguration.Named(b); b.Property(x => x.VersionLabel).HasMaxLength(80).IsRequired(); b.Property(x => x.Status).HasConversion<string>().HasMaxLength(24); b.HasOne(x => x.EducationProgram).WithMany(x => x.CurriculumPlans).HasForeignKey(x => x.EducationProgramId).OnDelete(DeleteBehavior.Restrict); }
}
internal sealed class CurriculumGradeSubjectConfiguration : IEntityTypeConfiguration<CurriculumGradeSubject>
{
    public void Configure(EntityTypeBuilder<CurriculumGradeSubject> b) { b.ToTable("curriculum_grade_subjects"); b.HasKey(x => x.Id); AcademicConfiguration.Common(b); b.Property(x => x.InstructionLanguage).HasMaxLength(40); b.HasOne(x => x.CurriculumPlan).WithMany(x => x.GradeSubjects).HasForeignKey(x => x.CurriculumPlanId).OnDelete(DeleteBehavior.Restrict); b.HasOne(x => x.GradeLevel).WithMany(x => x.CurriculumSubjects).HasForeignKey(x => x.GradeLevelId).OnDelete(DeleteBehavior.Restrict); b.HasOne(x => x.Subject).WithMany(x => x.CurriculumGrades).HasForeignKey(x => x.SubjectId).OnDelete(DeleteBehavior.Restrict); }
}
internal sealed class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> b) { b.ToTable("books"); b.HasKey(x => x.Id); AcademicConfiguration.Named(b); b.Property(x => x.Publisher).HasMaxLength(200); }
}
internal sealed class BookVersionConfiguration : IEntityTypeConfiguration<BookVersion>
{
    public void Configure(EntityTypeBuilder<BookVersion> b) { b.ToTable("book_versions"); b.HasKey(x => x.Id); AcademicConfiguration.Common(b); b.Property(x => x.EditionCode).HasMaxLength(50).IsRequired(); b.Property(x => x.VersionLabel).HasMaxLength(100).IsRequired(); b.Property(x => x.Language).HasMaxLength(40).IsRequired(); b.Property(x => x.Isbn).HasMaxLength(32); b.HasOne(x => x.Book).WithMany(x => x.Versions).HasForeignKey(x => x.BookId).OnDelete(DeleteBehavior.Restrict); }
}
internal sealed class BookRoleConfiguration : IEntityTypeConfiguration<BookRole>
{
    private static readonly DateTimeOffset SeedDate = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    public void Configure(EntityTypeBuilder<BookRole> b)
    {
        b.ToTable("book_roles"); b.HasKey(x => x.Id); AcademicConfiguration.Named(b);
        b.HasData(
            Role("b1000000-0000-0000-0000-000000000001", "PRIMARY", "الكتاب الأساسي", "Primary book"),
            Role("b1000000-0000-0000-0000-000000000002", "WORKBOOK", "كتاب التدريبات", "Workbook"),
            Role("b1000000-0000-0000-0000-000000000003", "TEACHER_GUIDE", "دليل المعلم", "Teacher guide"),
            Role("b1000000-0000-0000-0000-000000000004", "SUPPLEMENTARY", "كتاب مساعد", "Supplementary book"),
            Role("b1000000-0000-0000-0000-000000000005", "REFERENCE", "مرجع", "Reference"),
            Role("b1000000-0000-0000-0000-000000000006", "ACTIVITY_BOOK", "كتاب الأنشطة", "Activity book"));
    }
    private static BookRole Role(string id, string code, string ar, string en) => new() { Id=Guid.Parse(id), Code=code, NameAr=ar, NameEn=en, IsSystem=true, IsActive=true, CreatedAtUtc=SeedDate, UpdatedAtUtc=SeedDate };
}
internal sealed class CurriculumSubjectBookConfiguration : IEntityTypeConfiguration<CurriculumSubjectBook>
{
    public void Configure(EntityTypeBuilder<CurriculumSubjectBook> b) { b.ToTable("curriculum_subject_books"); b.HasKey(x => x.Id); AcademicConfiguration.Common(b); b.HasOne(x => x.CurriculumGradeSubject).WithMany(x => x.Books).HasForeignKey(x => x.CurriculumGradeSubjectId).OnDelete(DeleteBehavior.Restrict); b.HasOne(x => x.BookVersion).WithMany(x => x.CurriculumSubjects).HasForeignKey(x => x.BookVersionId).OnDelete(DeleteBehavior.Restrict); b.HasOne(x => x.BookRole).WithMany(x => x.CurriculumSubjects).HasForeignKey(x => x.BookRoleId).OnDelete(DeleteBehavior.Restrict); }
}
internal sealed class GradeSubjectOfferingConfiguration : IEntityTypeConfiguration<GradeSubjectOffering>
{
    public void Configure(EntityTypeBuilder<GradeSubjectOffering> b) { b.ToTable("grade_subject_offerings"); b.HasKey(x => x.Id); AcademicConfiguration.Common(b); b.Property(x => x.Status).HasConversion<string>().HasMaxLength(24); b.HasOne(x => x.GradeOffering).WithMany(x => x.SubjectOfferings).HasForeignKey(x => x.GradeOfferingId).OnDelete(DeleteBehavior.Restrict); b.HasOne(x => x.CurriculumGradeSubject).WithMany(x => x.Offerings).HasForeignKey(x => x.CurriculumGradeSubjectId).OnDelete(DeleteBehavior.Restrict); }
}
internal sealed class TeacherGradeSubjectScopeConfiguration : IEntityTypeConfiguration<TeacherGradeSubjectScope>
{
    public void Configure(EntityTypeBuilder<TeacherGradeSubjectScope> b) { b.ToTable("teacher_grade_subject_scopes"); b.HasKey(x => x.Id); AcademicConfiguration.Common(b); b.HasOne(x => x.TeacherUser).WithMany().HasForeignKey(x => x.TeacherUserId).OnDelete(DeleteBehavior.Restrict); b.HasOne(x => x.GradeSubjectOffering).WithMany().HasForeignKey(x => x.GradeSubjectOfferingId).OnDelete(DeleteBehavior.Restrict); }
}
internal sealed class ClassSectionTeacherScopeConfiguration : IEntityTypeConfiguration<ClassSectionTeacherScope>
{
    public void Configure(EntityTypeBuilder<ClassSectionTeacherScope> b) { b.ToTable("class_section_teacher_scopes"); b.HasKey(x => x.Id); AcademicConfiguration.Common(b); b.HasOne(x => x.ClassSection).WithMany().HasForeignKey(x => x.ClassSectionId).OnDelete(DeleteBehavior.Restrict); b.HasOne(x => x.TeacherGradeSubjectScope).WithMany().HasForeignKey(x => x.TeacherGradeSubjectScopeId).OnDelete(DeleteBehavior.Restrict); }
}
internal sealed class ClassSectionSubjectConfiguration : IEntityTypeConfiguration<ClassSectionSubject>
{
    public void Configure(EntityTypeBuilder<ClassSectionSubject> b) { b.ToTable("class_section_subjects"); b.HasKey(x => x.Id); AcademicConfiguration.Common(b); b.HasOne(x => x.ClassSection).WithMany().HasForeignKey(x => x.ClassSectionId).OnDelete(DeleteBehavior.Restrict); b.HasOne(x => x.GradeSubjectOffering).WithMany().HasForeignKey(x => x.GradeSubjectOfferingId).OnDelete(DeleteBehavior.Restrict); }
}
internal sealed class ClassSubjectTeacherAssignmentConfiguration : IEntityTypeConfiguration<ClassSubjectTeacherAssignment>
{
    public void Configure(EntityTypeBuilder<ClassSubjectTeacherAssignment> b) { b.ToTable("class_subject_teacher_assignments"); b.HasKey(x => x.Id); AcademicConfiguration.Common(b); b.Property(x => x.Role).HasConversion<string>().HasMaxLength(24); b.HasOne(x => x.ClassSectionSubject).WithMany(x => x.TeacherAssignments).HasForeignKey(x => x.ClassSectionSubjectId).OnDelete(DeleteBehavior.Restrict); b.HasOne(x => x.TeacherGradeSubjectScope).WithMany().HasForeignKey(x => x.TeacherGradeSubjectScopeId).OnDelete(DeleteBehavior.Restrict); }
}
internal sealed class WeeklyTimetableSlotConfiguration : IEntityTypeConfiguration<WeeklyTimetableSlot>
{
    public void Configure(EntityTypeBuilder<WeeklyTimetableSlot> b) { b.ToTable("weekly_timetable_slots"); b.HasKey(x => x.Id); AcademicConfiguration.Common(b); b.Property(x => x.DayOfWeek).HasConversion<string>().HasMaxLength(16); b.HasOne(x => x.ClassSection).WithMany(x => x.TimetableSlots).HasForeignKey(x => x.ClassSectionId).OnDelete(DeleteBehavior.Restrict); b.HasOne(x => x.ClassSectionSubject).WithMany(x => x.TimetableSlots).HasForeignKey(x => x.ClassSectionSubjectId).OnDelete(DeleteBehavior.Restrict); b.HasOne(x => x.PrimaryTeacherScope).WithMany().HasForeignKey(x => x.PrimaryTeacherScopeId).OnDelete(DeleteBehavior.Restrict); b.HasOne(x => x.Room).WithMany().HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Restrict); }
}
internal sealed class TemporaryClassMergeConfiguration : IEntityTypeConfiguration<TemporaryClassMerge>
{
    public void Configure(EntityTypeBuilder<TemporaryClassMerge> b) { b.ToTable("temporary_class_merges"); b.HasKey(x => x.Id); AcademicConfiguration.Common(b); b.Property(x => x.Notes).HasMaxLength(500); b.HasOne(x => x.Room).WithMany().HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Restrict); }
}
internal sealed class TemporaryClassMergeSectionConfiguration : IEntityTypeConfiguration<TemporaryClassMergeSection>
{
    public void Configure(EntityTypeBuilder<TemporaryClassMergeSection> b) { b.ToTable("temporary_class_merge_sections"); b.HasKey(x => x.Id); AcademicConfiguration.Common(b); b.HasOne(x => x.TemporaryClassMerge).WithMany(x => x.Sections).HasForeignKey(x => x.TemporaryClassMergeId).OnDelete(DeleteBehavior.Restrict); b.HasOne(x => x.ClassSection).WithMany().HasForeignKey(x => x.ClassSectionId).OnDelete(DeleteBehavior.Restrict); }
}
internal sealed class WeeklyTimetableSlotSubstituteTeacherConfiguration : IEntityTypeConfiguration<WeeklyTimetableSlotSubstituteTeacher>
{
    public void Configure(EntityTypeBuilder<WeeklyTimetableSlotSubstituteTeacher> b) { b.ToTable("weekly_timetable_slot_substitute_teachers"); b.HasKey(x => x.Id); AcademicConfiguration.Common(b); b.HasOne(x => x.WeeklyTimetableSlot).WithMany(x => x.SubstituteTeachers).HasForeignKey(x => x.WeeklyTimetableSlotId).OnDelete(DeleteBehavior.Restrict); b.HasOne(x => x.TeacherGradeSubjectScope).WithMany().HasForeignKey(x => x.TeacherGradeSubjectScopeId).OnDelete(DeleteBehavior.Restrict); }
}
internal sealed class TeacherSubstitutionConfiguration : IEntityTypeConfiguration<TeacherSubstitution>
{
    public void Configure(EntityTypeBuilder<TeacherSubstitution> b) { b.ToTable("teacher_substitutions"); b.HasKey(x => x.Id); AcademicConfiguration.Common(b); b.Property(x => x.Reason).HasMaxLength(500); b.Property(x => x.SourceType).HasMaxLength(50).IsRequired(); b.Property(x => x.SourceReferenceId).HasMaxLength(100); b.HasOne(x => x.WeeklyTimetableSlot).WithMany().HasForeignKey(x => x.WeeklyTimetableSlotId).OnDelete(DeleteBehavior.Restrict); b.HasOne(x => x.SubstituteTeacherAssignment).WithMany().HasForeignKey(x => x.SubstituteTeacherAssignmentId).OnDelete(DeleteBehavior.Restrict); }
}
