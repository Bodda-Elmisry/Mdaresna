using Mdaresna.Schools.Domain.Students;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Schools.Infrastructure.Persistence;

internal sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> b) { b.ToTable("students"); b.HasKey(x=>x.Id); b.Property(x=>x.StudentCode).HasMaxLength(32).IsRequired(); b.Property(x=>x.FullNameAr).HasMaxLength(200).IsRequired(); b.Property(x=>x.FullNameEn).HasMaxLength(200).IsRequired(); b.Property(x=>x.NormalizedName).HasMaxLength(200).IsRequired(); b.Property(x=>x.Gender).HasConversion<string>().HasMaxLength(16); b.Property(x=>x.NationalId).HasMaxLength(40); b.Property(x=>x.BirthCertificateNumber).HasMaxLength(80); b.HasIndex(x=>x.PersonId).IsUnique(); b.HasIndex(x=>x.GlobalStudentId).IsUnique(); b.HasIndex(x=>x.StudentCode).IsUnique(); b.HasIndex(x=>new{x.NormalizedName,x.DateOfBirth}); b.HasOne(x=>x.Person).WithOne().HasForeignKey<Student>(x=>x.PersonId).OnDelete(DeleteBehavior.Restrict); }
}
internal sealed class GuardianConfiguration : IEntityTypeConfiguration<Guardian>
{
    public void Configure(EntityTypeBuilder<Guardian> b) { b.ToTable("guardians"); b.HasKey(x=>x.Id); b.Property(x=>x.FullName).HasMaxLength(200).IsRequired(); b.Property(x=>x.Phone).HasMaxLength(20).IsRequired(); b.Property(x=>x.Email).HasMaxLength(254); b.Property(x=>x.NationalId).HasMaxLength(40); b.HasIndex(x=>x.Phone); }
}
internal sealed class StudentGuardianConfiguration : IEntityTypeConfiguration<StudentGuardian>
{
    public void Configure(EntityTypeBuilder<StudentGuardian> b) { b.ToTable("student_guardians"); b.HasKey(x=>x.Id); b.Property(x=>x.Relationship).HasConversion<string>().HasMaxLength(24); b.HasOne(x=>x.Student).WithMany(x=>x.Guardians).HasForeignKey(x=>x.StudentId).OnDelete(DeleteBehavior.Cascade); b.HasOne(x=>x.Guardian).WithMany(x=>x.Students).HasForeignKey(x=>x.GuardianId).OnDelete(DeleteBehavior.Restrict); b.HasIndex(x=>new{x.StudentId,x.GuardianId}).IsUnique(); }
}
internal sealed class StudentEnrollmentConfiguration : IEntityTypeConfiguration<StudentEnrollment>
{
    public void Configure(EntityTypeBuilder<StudentEnrollment> b) { b.ToTable("student_enrollments"); b.HasKey(x=>x.Id); b.Property(x=>x.Status).HasConversion<string>().HasMaxLength(24); b.HasOne(x=>x.Student).WithMany(x=>x.Enrollments).HasForeignKey(x=>x.StudentId).OnDelete(DeleteBehavior.Restrict); b.HasOne(x=>x.GradeOffering).WithMany().HasForeignKey(x=>x.GradeOfferingId).OnDelete(DeleteBehavior.Restrict); b.HasOne(x=>x.ClassSection).WithMany().HasForeignKey(x=>x.ClassSectionId).OnDelete(DeleteBehavior.Restrict); b.HasIndex(x=>new{x.StudentId,x.GradeOfferingId}).IsUnique(); b.HasIndex(x=>new{x.ClassSectionId,x.Status}); }
}
internal sealed class StudentAttendanceRegisterConfiguration : IEntityTypeConfiguration<StudentAttendanceRegister>
{
    public void Configure(EntityTypeBuilder<StudentAttendanceRegister> b)
    {
        b.ToTable("student_attendance_registers"); b.HasKey(x => x.Id);
        b.Property(x => x.UnitKey).HasMaxLength(64).IsRequired();
        b.Property(x => x.Mode).HasConversion<string>().HasMaxLength(24);
        b.Property(x => x.TimeZoneIdSnapshot).HasMaxLength(100).IsRequired();
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(24);
        b.HasIndex(x => new { x.ClassSectionId, x.AttendanceDate, x.UnitKey }).IsUnique();
        b.HasOne(x => x.ClassSection).WithMany().HasForeignKey(x => x.ClassSectionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.WeeklyTimetableSlot).WithMany().HasForeignKey(x => x.WeeklyTimetableSlotId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.RecordedByUser).WithMany().HasForeignKey(x => x.RecordedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.FinalizedByUser).WithMany().HasForeignKey(x => x.FinalizedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
internal sealed class StudentAttendanceEntryConfiguration : IEntityTypeConfiguration<StudentAttendanceEntry>
{
    public void Configure(EntityTypeBuilder<StudentAttendanceEntry> b)
    {
        b.ToTable("student_attendance_entries"); b.HasKey(x => x.Id);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(24);
        b.Property(x => x.Notes).HasMaxLength(1000);
        b.HasIndex(x => new { x.RegisterId, x.StudentEnrollmentId }).IsUnique();
        b.HasOne(x => x.Register).WithMany(x => x.Entries).HasForeignKey(x => x.RegisterId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.StudentEnrollment).WithMany().HasForeignKey(x => x.StudentEnrollmentId).OnDelete(DeleteBehavior.Restrict);
    }
}
internal sealed class StudentAttendanceAuditConfiguration : IEntityTypeConfiguration<StudentAttendanceAudit>
{
    public void Configure(EntityTypeBuilder<StudentAttendanceAudit> b)
    {
        b.ToTable("student_attendance_audits"); b.HasKey(x => x.Id);
        b.Property(x => x.Action).HasMaxLength(40).IsRequired();
        b.Property(x => x.SnapshotJson);
        b.HasIndex(x => new { x.RegisterId, x.CreatedAtUtc });
        b.HasOne(x => x.Register).WithMany(x => x.AuditTrail).HasForeignKey(x => x.RegisterId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.ActorUser).WithMany().HasForeignKey(x => x.ActorUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
internal sealed class AdmissionApplicationConfiguration : IEntityTypeConfiguration<AdmissionApplication>
{
    public void Configure(EntityTypeBuilder<AdmissionApplication> b) { b.ToTable("admission_applications"); b.HasKey(x=>x.Id); b.Property(x=>x.ApplicationNumber).HasMaxLength(40).IsRequired(); b.Property(x=>x.StudentCode).HasMaxLength(32); b.Property(x=>x.FullNameAr).HasMaxLength(200).IsRequired(); b.Property(x=>x.FullNameEn).HasMaxLength(200).IsRequired(); b.Property(x=>x.NormalizedName).HasMaxLength(200).IsRequired(); b.Property(x=>x.Gender).HasConversion<string>().HasMaxLength(16); b.Property(x=>x.NationalId).HasMaxLength(40); b.Property(x=>x.BirthCertificateNumber).HasMaxLength(80); b.Property(x=>x.Source).HasConversion<string>().HasMaxLength(24); b.Property(x=>x.Status).HasConversion<string>().HasMaxLength(32); b.Property(x=>x.Notes).HasMaxLength(2000); b.HasOne(x=>x.ProgramAcademicYear).WithMany().HasForeignKey(x=>x.ProgramAcademicYearId).OnDelete(DeleteBehavior.Restrict); b.HasOne(x=>x.GradeLevel).WithMany().HasForeignKey(x=>x.GradeLevelId).OnDelete(DeleteBehavior.Restrict); b.HasOne(x=>x.AcceptedStudent).WithMany().HasForeignKey(x=>x.AcceptedStudentId).OnDelete(DeleteBehavior.Restrict); b.HasIndex(x=>x.ApplicationNumber).IsUnique(); b.HasIndex(x=>new{x.Status,x.SubmittedAtUtc}); b.HasIndex(x=>new{x.ProgramAcademicYearId,x.GradeLevelId}); b.HasIndex(x=>new{x.NormalizedName,x.DateOfBirth}); }
}
internal sealed class AdmissionApplicationGuardianConfiguration : IEntityTypeConfiguration<AdmissionApplicationGuardian>
{
    public void Configure(EntityTypeBuilder<AdmissionApplicationGuardian> b) { b.ToTable("admission_application_guardians"); b.HasKey(x=>x.Id); b.Property(x=>x.Relationship).HasConversion<string>().HasMaxLength(24); b.HasOne(x=>x.AdmissionApplication).WithMany(x=>x.Guardians).HasForeignKey(x=>x.AdmissionApplicationId).OnDelete(DeleteBehavior.Cascade); b.HasOne(x=>x.Guardian).WithMany(x=>x.Applications).HasForeignKey(x=>x.GuardianId).OnDelete(DeleteBehavior.Restrict); b.HasIndex(x=>new{x.AdmissionApplicationId,x.GuardianId}).IsUnique(); }
}
