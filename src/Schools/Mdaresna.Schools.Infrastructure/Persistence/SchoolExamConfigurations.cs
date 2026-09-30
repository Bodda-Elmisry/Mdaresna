using Mdaresna.Schools.Domain.Exams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Schools.Infrastructure.Persistence;

internal sealed class ExamPolicyConfiguration : IEntityTypeConfiguration<ExamPolicy>
{
    public void Configure(EntityTypeBuilder<ExamPolicy> b)
    {
        b.ToTable("exam_policies"); b.HasKey(x => x.Id);
        b.Property(x => x.ExamKind).HasConversion<string>().HasMaxLength(24);
        b.Property(x => x.DefaultAdministrationMode).HasConversion<string>().HasMaxLength(24);
        b.HasIndex(x => new { x.EducationProgramId, x.EducationStageId, x.ExamKind, x.Version }).IsUnique();
        b.HasOne(x => x.EducationProgram).WithMany().HasForeignKey(x => x.EducationProgramId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.EducationStage).WithMany().HasForeignKey(x => x.EducationStageId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ExamSeriesConfiguration : IEntityTypeConfiguration<ExamSeries>
{
    public void Configure(EntityTypeBuilder<ExamSeries> b)
    {
        b.ToTable("exam_series"); b.HasKey(x => x.Id); b.HasQueryFilter(x => !x.IsDeleted);
        b.Property(x => x.Code).HasMaxLength(60).IsRequired();
        b.Property(x => x.NameAr).HasMaxLength(250).IsRequired(); b.Property(x => x.NameEn).HasMaxLength(250).IsRequired();
        b.Property(x => x.Kind).HasConversion<string>().HasMaxLength(24);
        b.Property(x => x.ScopeLevel).HasConversion<string>().HasMaxLength(24);
        b.Property(x => x.Purpose).HasConversion<string>().HasMaxLength(24);
        b.Property(x => x.IssuingAuthority).HasConversion<string>().HasMaxLength(32);
        b.Property(x => x.SchedulingAuthority).HasConversion<string>().HasMaxLength(32);
        b.Property(x => x.DefaultAdministrationMode).HasConversion<string>().HasMaxLength(24);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(24);
        b.Property(x => x.TimeZoneIdSnapshot).HasMaxLength(100).IsRequired();
        b.Property(x => x.ExternalSourceCode).HasMaxLength(100); b.Property(x => x.ExternalAuthorityName).HasMaxLength(250);
        b.Property(x => x.ExternalReferenceId).HasMaxLength(200); b.Property(x => x.ExternalRevision).HasMaxLength(100);
        b.Property(x => x.ExternalPayloadHash).HasMaxLength(128); b.Property(x => x.CancellationReason).HasMaxLength(1000);
        b.HasIndex(x => new { x.ProgramAcademicYearId, x.Code }).IsUnique();
        b.HasIndex(x => new { x.Status, x.Kind, x.CreatedAtUtc });
        b.HasIndex(x => new { x.ProgramAcademicYearId, x.AcademicTermId, x.AssessmentMonth, x.Kind });
        b.HasIndex(x => new { x.ScopeLevel, x.EducationStageId, x.ScopeGradeOfferingId, x.ScopeClassSectionId });
        b.HasIndex(x => new { x.ExternalSourceCode, x.ExternalReferenceId, x.ExternalRevision });
        b.HasOne(x => x.ProgramAcademicYear).WithMany().HasForeignKey(x => x.ProgramAcademicYearId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.AcademicTerm).WithMany().HasForeignKey(x => x.AcademicTermId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.EducationStage).WithMany().HasForeignKey(x => x.EducationStageId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ScopeGradeOffering).WithMany().HasForeignKey(x => x.ScopeGradeOfferingId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ScopeClassSection).WithMany().HasForeignKey(x => x.ScopeClassSectionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ParentExamSeries).WithMany().HasForeignKey(x => x.ParentExamSeriesId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ExamSeriesTargetConfiguration : IEntityTypeConfiguration<ExamSeriesTarget>
{
    public void Configure(EntityTypeBuilder<ExamSeriesTarget> b)
    {
        b.ToTable("exam_series_targets"); b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.ExamSeriesId, x.GradeOfferingId, x.ClassSectionId }).IsUnique();
        b.HasOne(x => x.ExamSeries).WithMany(x => x.Targets).HasForeignKey(x => x.ExamSeriesId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.GradeOffering).WithMany().HasForeignKey(x => x.GradeOfferingId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ClassSection).WithMany().HasForeignKey(x => x.ClassSectionId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ExamPaperConfiguration : IEntityTypeConfiguration<ExamPaper>
{
    public void Configure(EntityTypeBuilder<ExamPaper> b)
    {
        b.ToTable("exam_papers"); b.HasKey(x => x.Id);
        b.Property(x => x.PaperCode).HasMaxLength(60).IsRequired(); b.Property(x => x.TitleAr).HasMaxLength(250).IsRequired();
        b.Property(x => x.TitleEn).HasMaxLength(250).IsRequired(); b.Property(x => x.Format).HasConversion<string>().HasMaxLength(24);
        b.Property(x => x.Instructions).HasMaxLength(6000); b.Property(x => x.TotalScore).HasPrecision(10, 2);
        b.Property(x => x.PassScore).HasPrecision(10, 2); b.Property(x => x.ResultsStatus).HasConversion<string>().HasMaxLength(24);
        b.Property(x => x.SubjectCodeSnapshot).HasMaxLength(60); b.Property(x => x.SubjectNameArSnapshot).HasMaxLength(200);
        b.Property(x => x.SubjectNameEnSnapshot).HasMaxLength(200); b.Property(x => x.GradeNameArSnapshot).HasMaxLength(200);
        b.Property(x => x.GradeNameEnSnapshot).HasMaxLength(200);
        b.HasIndex(x => new { x.ExamSeriesId, x.PaperCode }).IsUnique();
        b.HasOne(x => x.ExamSeries).WithMany(x => x.Papers).HasForeignKey(x => x.ExamSeriesId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.GradeSubjectOffering).WithMany().HasForeignKey(x => x.GradeSubjectOfferingId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ContentOwnerUser).WithMany().HasForeignKey(x => x.ContentOwnerUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ExamPaperTargetConfiguration : IEntityTypeConfiguration<ExamPaperTarget>
{
    public void Configure(EntityTypeBuilder<ExamPaperTarget> b)
    {
        b.ToTable("exam_paper_targets"); b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.ExamPaperId, x.ClassSectionId, x.ClassSectionSubjectId }).IsUnique();
        b.HasOne(x => x.ExamPaper).WithMany(x => x.Targets).HasForeignKey(x => x.ExamPaperId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.GradeOffering).WithMany().HasForeignKey(x => x.GradeOfferingId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ClassSection).WithMany().HasForeignKey(x => x.ClassSectionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ClassSectionSubject).WithMany().HasForeignKey(x => x.ClassSectionSubjectId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ExamScheduleWindowConfiguration : IEntityTypeConfiguration<ExamScheduleWindow>
{
    public void Configure(EntityTypeBuilder<ExamScheduleWindow> b)
    {
        b.ToTable("exam_schedule_windows"); b.HasKey(x => x.Id);
        b.Property(x => x.TimeZoneIdSnapshot).HasMaxLength(100).IsRequired(); b.Property(x => x.Status).HasConversion<string>().HasMaxLength(24);
        b.Property(x => x.PostponementReason).HasMaxLength(1000); b.Property(x => x.CancellationReason).HasMaxLength(1000);
        b.HasIndex(x => new { x.LocalDate, x.StartsAtUtc, x.EndsAtUtc, x.Status });
        b.HasOne(x => x.ExamSeries).WithMany(x => x.ScheduleWindows).HasForeignKey(x => x.ExamSeriesId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.RescheduledFromWindow).WithMany().HasForeignKey(x => x.RescheduledFromWindowId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ExamSittingConfiguration : IEntityTypeConfiguration<ExamSitting>
{
    public void Configure(EntityTypeBuilder<ExamSitting> b)
    {
        b.ToTable("exam_sittings"); b.HasKey(x => x.Id); b.HasAlternateKey(x => new { x.Id, x.ExamPaperId });
        b.Property(x => x.Purpose).HasConversion<string>().HasMaxLength(24); b.Property(x => x.AdministrationMode).HasConversion<string>().HasMaxLength(24);
        b.Property(x => x.ExecutionStatus).HasConversion<string>().HasMaxLength(24);
        b.HasIndex(x => new { x.ExamScheduleWindowId, x.ExamPaperId }).IsUnique();
        b.HasOne(x => x.ExamPaper).WithMany(x => x.Sittings).HasForeignKey(x => x.ExamPaperId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ExamScheduleWindow).WithMany(x => x.Sittings).HasForeignKey(x => x.ExamScheduleWindowId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ParentSitting).WithMany().HasForeignKey(x => x.ParentSittingId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ExamWindowVenueConfiguration : IEntityTypeConfiguration<ExamWindowVenue>
{
    public void Configure(EntityTypeBuilder<ExamWindowVenue> b)
    {
        b.ToTable("exam_window_venues"); b.HasKey(x => x.Id);
        b.Property(x => x.VenueLabel).HasMaxLength(250); b.HasIndex(x => new { x.ExamScheduleWindowId, x.ClassSectionId });
        b.HasIndex(x => new { x.ExamScheduleWindowId, x.ExamCommitteeId }).IsUnique();
        b.HasOne(x => x.ExamScheduleWindow).WithMany(x => x.Venues).HasForeignKey(x => x.ExamScheduleWindowId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Room).WithMany().HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ClassSection).WithMany().HasForeignKey(x => x.ClassSectionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ExamCommittee).WithMany(x => x.Venues).HasForeignKey(x => x.ExamCommitteeId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ExamSittingVenueConfiguration : IEntityTypeConfiguration<ExamSittingVenue>
{
    public void Configure(EntityTypeBuilder<ExamSittingVenue> b)
    {
        b.ToTable("exam_sitting_venues"); b.HasKey(x => x.Id); b.HasAlternateKey(x => new { x.Id, x.ExamSittingId });
        b.HasIndex(x => new { x.ExamSittingId, x.ExamWindowVenueId }).IsUnique();
        b.HasOne(x => x.ExamSitting).WithMany(x => x.Venues).HasForeignKey(x => x.ExamSittingId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ExamWindowVenue).WithMany(x => x.Sittings).HasForeignKey(x => x.ExamWindowVenueId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ExamCandidateConfiguration : IEntityTypeConfiguration<ExamCandidate>
{
    public void Configure(EntityTypeBuilder<ExamCandidate> b)
    {
        b.ToTable("exam_candidates"); b.HasKey(x => x.Id); b.Property(x => x.Status).HasConversion<string>().HasMaxLength(24);
        b.Property(x => x.StudentCodeSnapshot).HasMaxLength(60).IsRequired(); b.Property(x => x.NameArSnapshot).HasMaxLength(250).IsRequired();
        b.Property(x => x.NameEnSnapshot).HasMaxLength(250).IsRequired(); b.Property(x => x.ExamNumber).HasMaxLength(60);
        b.HasIndex(x => new { x.ExamSeriesId, x.StudentId }).IsUnique(); b.HasIndex(x => new { x.ExamSeriesId, x.ExamNumber });
        b.HasOne(x => x.ExamSeries).WithMany(x => x.Candidates).HasForeignKey(x => x.ExamSeriesId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.StudentEnrollmentSnapshot).WithMany().HasForeignKey(x => x.StudentEnrollmentIdSnapshot).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ExamPaperCandidateConfiguration : IEntityTypeConfiguration<ExamPaperCandidate>
{
    public void Configure(EntityTypeBuilder<ExamPaperCandidate> b)
    {
        b.ToTable("exam_paper_candidates"); b.HasKey(x => x.Id); b.HasAlternateKey(x => new { x.Id, x.ExamPaperId });
        b.Property(x => x.Eligibility).HasConversion<string>().HasMaxLength(24); b.HasIndex(x => new { x.ExamPaperId, x.ExamCandidateId }).IsUnique();
        b.HasOne(x => x.ExamPaper).WithMany(x => x.Candidates).HasForeignKey(x => x.ExamPaperId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ExamCandidate).WithMany(x => x.Papers).HasForeignKey(x => x.ExamCandidateId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ExamCandidateSittingAssignmentConfiguration : IEntityTypeConfiguration<ExamCandidateSittingAssignment>
{
    public void Configure(EntityTypeBuilder<ExamCandidateSittingAssignment> b)
    {
        b.ToTable("exam_candidate_sitting_assignments"); b.HasKey(x => x.Id); b.HasAlternateKey(x => new { x.Id, x.ExamPaperCandidateId });
        b.Property(x => x.DeskOrSeatLabel).HasMaxLength(60); b.HasIndex(x => new { x.ExamSittingId, x.ExamPaperCandidateId }).IsUnique();
        b.HasOne(x => x.ExamPaperCandidate).WithMany(x => x.SittingAssignments)
            .HasForeignKey(x => new { x.ExamPaperCandidateId, x.ExamPaperId }).HasPrincipalKey(x => new { x.Id, x.ExamPaperId }).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ExamSitting).WithMany(x => x.CandidateAssignments)
            .HasForeignKey(x => new { x.ExamSittingId, x.ExamPaperId }).HasPrincipalKey(x => new { x.Id, x.ExamPaperId }).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ExamSittingVenue).WithMany(x => x.CandidateAssignments)
            .HasForeignKey(x => new { x.ExamSittingVenueId, x.ExamSittingId }).HasPrincipalKey(x => new { x.Id, x.ExamSittingId }).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ExamAttendanceConfiguration : IEntityTypeConfiguration<ExamAttendance>
{
    public void Configure(EntityTypeBuilder<ExamAttendance> b)
    {
        b.ToTable("exam_attendance"); b.HasKey(x => x.Id); b.Property(x => x.Status).HasConversion<string>().HasMaxLength(24);
        b.Property(x => x.Notes).HasMaxLength(2000); b.HasIndex(x => x.ExamCandidateSittingAssignmentId).IsUnique();
        b.HasOne(x => x.ExamCandidateSittingAssignment).WithOne(x => x.Attendance).HasForeignKey<ExamAttendance>(x => x.ExamCandidateSittingAssignmentId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ExamResultAttemptConfiguration : IEntityTypeConfiguration<ExamResultAttempt>
{
    public void Configure(EntityTypeBuilder<ExamResultAttempt> b)
    {
        b.ToTable("exam_result_attempts"); b.HasKey(x => x.Id); b.Property(x => x.Disposition).HasConversion<string>().HasMaxLength(24);
        b.Property(x => x.Score).HasPrecision(10, 2); b.Property(x => x.Notes).HasMaxLength(2000);
        b.HasIndex(x => new { x.ExamPaperCandidateId, x.AttemptNumber }).IsUnique();
        b.HasOne(x => x.ExamPaperCandidate).WithMany(x => x.ResultAttempts).HasForeignKey(x => x.ExamPaperCandidateId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ExamCandidateSittingAssignment).WithMany(x => x.ResultAttempts).HasForeignKey(x => x.ExamCandidateSittingAssignmentId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ExamCommitteeConfiguration : IEntityTypeConfiguration<ExamCommittee>
{
    public void Configure(EntityTypeBuilder<ExamCommittee> b)
    {
        b.ToTable("exam_committees"); b.HasKey(x => x.Id);
        b.Property(x => x.Code).HasMaxLength(60).IsRequired(); b.Property(x => x.NameAr).HasMaxLength(250).IsRequired();
        b.Property(x => x.NameEn).HasMaxLength(250).IsRequired(); b.HasIndex(x => new { x.ExamSeriesId, x.Code }).IsUnique();
        b.HasIndex(x => new { x.ExamScheduleWindowId, x.SortOrder }).IsUnique();
        b.HasOne(x => x.ExamSeries).WithMany(x => x.Committees).HasForeignKey(x => x.ExamSeriesId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ExamScheduleWindow).WithMany(x => x.Committees).HasForeignKey(x => x.ExamScheduleWindowId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ExamInvigilatorAssignmentConfiguration : IEntityTypeConfiguration<ExamInvigilatorAssignment>
{
    public void Configure(EntityTypeBuilder<ExamInvigilatorAssignment> b)
    {
        b.ToTable("exam_invigilator_assignments"); b.HasKey(x => x.Id);
        b.Property(x => x.Role).HasConversion<string>().HasMaxLength(24); b.Property(x => x.Status).HasConversion<string>().HasMaxLength(24);
        b.Property(x => x.Notes).HasMaxLength(1000); b.HasIndex(x => new { x.ExamCommitteeId, x.UserId }).IsUnique();
        b.HasIndex(x => new { x.UserId, x.Status });
        b.HasOne(x => x.ExamCommittee).WithMany(x => x.Invigilators).HasForeignKey(x => x.ExamCommitteeId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ReplacesAssignment).WithMany().HasForeignKey(x => x.ReplacesAssignmentId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ExamResultAppealConfiguration : IEntityTypeConfiguration<ExamResultAppeal>
{
    public void Configure(EntityTypeBuilder<ExamResultAppeal> b)
    {
        b.ToTable("exam_result_appeals"); b.HasKey(x => x.Id);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(24); b.Property(x => x.Reason).HasMaxLength(2000).IsRequired();
        b.Property(x => x.DecisionNotes).HasMaxLength(2000); b.Property(x => x.PreviousScore).HasPrecision(10, 2); b.Property(x => x.RevisedScore).HasPrecision(10, 2);
        b.HasIndex(x => new { x.ExamPaperCandidateId, x.Status });
        b.HasOne(x => x.ExamPaperCandidate).WithMany(x => x.Appeals).HasForeignKey(x => x.ExamPaperCandidateId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ExamApprovalConfiguration : IEntityTypeConfiguration<ExamApproval>
{
    public void Configure(EntityTypeBuilder<ExamApproval> b)
    {
        b.ToTable("exam_approvals"); b.HasKey(x => x.Id); b.Property(x => x.Stage).HasConversion<string>().HasMaxLength(24);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(24); b.Property(x => x.DecisionReason).HasMaxLength(1000);
        b.HasIndex(x => new { x.ExamSeriesId, x.ExamPaperId, x.Stage, x.StepOrder }).IsUnique();
        b.HasOne(x => x.ExamSeries).WithMany(x => x.Approvals).HasForeignKey(x => x.ExamSeriesId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ExamPaper).WithMany().HasForeignKey(x => x.ExamPaperId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ExamAuditConfiguration : IEntityTypeConfiguration<ExamAudit>
{
    public void Configure(EntityTypeBuilder<ExamAudit> b)
    {
        b.ToTable("exam_audits"); b.HasKey(x => x.Id); b.Property(x => x.Action).HasMaxLength(60).IsRequired();
        b.Property(x => x.Reason).HasMaxLength(1000); b.HasIndex(x => new { x.ExamSeriesId, x.CreatedAtUtc });
        b.HasOne(x => x.ExamSeries).WithMany(x => x.AuditTrail).HasForeignKey(x => x.ExamSeriesId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ActorUser).WithMany().HasForeignKey(x => x.ActorUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ExamCalendarProjectionConfiguration : IEntityTypeConfiguration<ExamCalendarProjection>
{
    public void Configure(EntityTypeBuilder<ExamCalendarProjection> b)
    {
        b.ToTable("exam_calendar_projections"); b.HasKey(x => x.Id);
        b.HasIndex(x => x.ExamScheduleWindowId).IsUnique(); b.HasIndex(x => x.SchoolCalendarEventId).IsUnique();
        b.HasOne(x => x.ExamScheduleWindow).WithOne(x => x.CalendarProjection).HasForeignKey<ExamCalendarProjection>(x => x.ExamScheduleWindowId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.SchoolCalendarEvent).WithMany().HasForeignKey(x => x.SchoolCalendarEventId).OnDelete(DeleteBehavior.Restrict);
    }
}
