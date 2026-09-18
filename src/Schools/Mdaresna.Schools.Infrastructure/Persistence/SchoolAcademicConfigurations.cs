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
    public void Configure(EntityTypeBuilder<ProgramAcademicYear> b) { b.ToTable("program_academic_years"); b.HasKey(x => x.Id); AcademicConfiguration.Named(b); b.HasOne(x => x.EducationProgram).WithMany(x => x.AcademicYears).HasForeignKey(x => x.EducationProgramId).OnDelete(DeleteBehavior.Restrict); b.HasOne(x => x.AcademicYearDefinition).WithMany(x => x.ProgramYears).HasForeignKey(x => x.AcademicYearDefinitionId).OnDelete(DeleteBehavior.Restrict); }
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
