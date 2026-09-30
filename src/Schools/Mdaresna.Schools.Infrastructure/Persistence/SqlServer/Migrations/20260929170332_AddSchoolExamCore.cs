using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mdaresna.Schools.Infrastructure.Persistence.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolExamCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "exam_policies",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EducationProgramId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EducationStageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExamKind = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: true),
                    DefaultAdministrationMode = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    TeacherCanCreate = table.Column<bool>(type: "bit", nullable: false),
                    TeacherCanPublishWithoutApproval = table.Column<bool>(type: "bit", nullable: false),
                    DepartmentApprovalRequired = table.Column<bool>(type: "bit", nullable: false),
                    SchoolApprovalRequired = table.Column<bool>(type: "bit", nullable: false),
                    ResultApprovalRequired = table.Column<bool>(type: "bit", nullable: false),
                    AllowScheduleWarningOverride = table.Column<bool>(type: "bit", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_policies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_exam_policies_education_programs_EducationProgramId",
                        column: x => x.EducationProgramId,
                        principalSchema: "school",
                        principalTable: "education_programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_policies_education_stages_EducationStageId",
                        column: x => x.EducationStageId,
                        principalSchema: "school",
                        principalTable: "education_stages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "exam_series",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProgramAcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicTermId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    IssuingAuthority = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    SchedulingAuthority = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    DefaultAdministrationMode = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    TimeZoneIdSnapshot = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PolicySnapshotJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExternalSourceCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ExternalAuthorityName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ExternalReferenceId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ExternalRevision = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ExternalPayloadHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    IsExternalScheduleLocked = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApprovedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PublishedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClosedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ApprovedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    PublishedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ClosedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CancelledAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_series", x => x.Id);
                    table.ForeignKey(
                        name: "FK_exam_series_academic_terms_AcademicTermId",
                        column: x => x.AcademicTermId,
                        principalSchema: "school",
                        principalTable: "academic_terms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_series_local_users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalSchema: "school",
                        principalTable: "local_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_series_program_academic_years_ProgramAcademicYearId",
                        column: x => x.ProgramAcademicYearId,
                        principalSchema: "school",
                        principalTable: "program_academic_years",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "exam_audits",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamSeriesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamPaperId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExamSittingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_audits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_exam_audits_exam_series_ExamSeriesId",
                        column: x => x.ExamSeriesId,
                        principalSchema: "school",
                        principalTable: "exam_series",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_audits_local_users_ActorUserId",
                        column: x => x.ActorUserId,
                        principalSchema: "school",
                        principalTable: "local_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "exam_candidates",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamSeriesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentEnrollmentIdSnapshot = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GlobalStudentIdSnapshot = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GradeOfferingIdSnapshot = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassSectionIdSnapshot = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentCodeSnapshot = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    NameArSnapshot = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    NameEnSnapshot = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_candidates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_exam_candidates_exam_series_ExamSeriesId",
                        column: x => x.ExamSeriesId,
                        principalSchema: "school",
                        principalTable: "exam_series",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_candidates_student_enrollments_StudentEnrollmentIdSnapshot",
                        column: x => x.StudentEnrollmentIdSnapshot,
                        principalSchema: "school",
                        principalTable: "student_enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_candidates_students_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "school",
                        principalTable: "students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "exam_papers",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamSeriesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GradeSubjectOfferingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaperCode = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    TitleAr = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    TitleEn = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Format = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    Instructions = table.Column<string>(type: "nvarchar(max)", maxLength: 6000, nullable: true),
                    TotalScore = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    PassScore = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    ContentOwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ResultsStatus = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    ResultsRevision = table.Column<int>(type: "int", nullable: false),
                    SubjectCodeSnapshot = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    SubjectNameArSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SubjectNameEnSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GradeNameArSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GradeNameEnSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_papers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_exam_papers_exam_series_ExamSeriesId",
                        column: x => x.ExamSeriesId,
                        principalSchema: "school",
                        principalTable: "exam_series",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_papers_grade_subject_offerings_GradeSubjectOfferingId",
                        column: x => x.GradeSubjectOfferingId,
                        principalSchema: "school",
                        principalTable: "grade_subject_offerings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_papers_local_users_ContentOwnerUserId",
                        column: x => x.ContentOwnerUserId,
                        principalSchema: "school",
                        principalTable: "local_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "exam_schedule_windows",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamSeriesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocalDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartsAtLocal = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndsAtLocal = table.Column<TimeOnly>(type: "time", nullable: false),
                    StartsAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EndsAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TimeZoneIdSnapshot = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    IsScheduleLocked = table.Column<bool>(type: "bit", nullable: false),
                    RescheduledFromWindowId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PostponementReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ScheduledByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_schedule_windows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_exam_schedule_windows_exam_schedule_windows_RescheduledFromWindowId",
                        column: x => x.RescheduledFromWindowId,
                        principalSchema: "school",
                        principalTable: "exam_schedule_windows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_schedule_windows_exam_series_ExamSeriesId",
                        column: x => x.ExamSeriesId,
                        principalSchema: "school",
                        principalTable: "exam_series",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "exam_series_targets",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamSeriesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GradeOfferingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassSectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_series_targets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_exam_series_targets_class_sections_ClassSectionId",
                        column: x => x.ClassSectionId,
                        principalSchema: "school",
                        principalTable: "class_sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_series_targets_exam_series_ExamSeriesId",
                        column: x => x.ExamSeriesId,
                        principalSchema: "school",
                        principalTable: "exam_series",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_series_targets_grade_offerings_GradeOfferingId",
                        column: x => x.GradeOfferingId,
                        principalSchema: "school",
                        principalTable: "grade_offerings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "exam_approvals",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamSeriesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamPaperId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Stage = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    StepOrder = table.Column<int>(type: "int", nullable: false),
                    ApproverUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    DecisionReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DecidedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_approvals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_exam_approvals_exam_papers_ExamPaperId",
                        column: x => x.ExamPaperId,
                        principalSchema: "school",
                        principalTable: "exam_papers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_approvals_exam_series_ExamSeriesId",
                        column: x => x.ExamSeriesId,
                        principalSchema: "school",
                        principalTable: "exam_series",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "exam_paper_candidates",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamPaperId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamCandidateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Eligibility = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_paper_candidates", x => x.Id);
                    table.UniqueConstraint("AK_exam_paper_candidates_Id_ExamPaperId", x => new { x.Id, x.ExamPaperId });
                    table.ForeignKey(
                        name: "FK_exam_paper_candidates_exam_candidates_ExamCandidateId",
                        column: x => x.ExamCandidateId,
                        principalSchema: "school",
                        principalTable: "exam_candidates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_paper_candidates_exam_papers_ExamPaperId",
                        column: x => x.ExamPaperId,
                        principalSchema: "school",
                        principalTable: "exam_papers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "exam_paper_targets",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamPaperId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GradeOfferingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassSectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClassSectionSubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_paper_targets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_exam_paper_targets_class_section_subjects_ClassSectionSubjectId",
                        column: x => x.ClassSectionSubjectId,
                        principalSchema: "school",
                        principalTable: "class_section_subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_paper_targets_class_sections_ClassSectionId",
                        column: x => x.ClassSectionId,
                        principalSchema: "school",
                        principalTable: "class_sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_paper_targets_exam_papers_ExamPaperId",
                        column: x => x.ExamPaperId,
                        principalSchema: "school",
                        principalTable: "exam_papers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_paper_targets_grade_offerings_GradeOfferingId",
                        column: x => x.GradeOfferingId,
                        principalSchema: "school",
                        principalTable: "grade_offerings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "exam_calendar_projections",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamScheduleWindowId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchoolCalendarEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectionVersion = table.Column<int>(type: "int", nullable: false),
                    LastProjectedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_calendar_projections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_exam_calendar_projections_exam_schedule_windows_ExamScheduleWindowId",
                        column: x => x.ExamScheduleWindowId,
                        principalSchema: "school",
                        principalTable: "exam_schedule_windows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_calendar_projections_school_calendar_events_SchoolCalendarEventId",
                        column: x => x.SchoolCalendarEventId,
                        principalSchema: "school",
                        principalTable: "school_calendar_events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "exam_sittings",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamPaperId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamScheduleWindowId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentSittingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Purpose = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    AdministrationMode = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    StartsAtOffsetMinutes = table.Column<int>(type: "int", nullable: false),
                    DurationMinutesSnapshot = table.Column<int>(type: "int", nullable: false),
                    ExecutionStatus = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    StartedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_sittings", x => x.Id);
                    table.UniqueConstraint("AK_exam_sittings_Id_ExamPaperId", x => new { x.Id, x.ExamPaperId });
                    table.ForeignKey(
                        name: "FK_exam_sittings_exam_papers_ExamPaperId",
                        column: x => x.ExamPaperId,
                        principalSchema: "school",
                        principalTable: "exam_papers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_sittings_exam_schedule_windows_ExamScheduleWindowId",
                        column: x => x.ExamScheduleWindowId,
                        principalSchema: "school",
                        principalTable: "exam_schedule_windows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_sittings_exam_sittings_ParentSittingId",
                        column: x => x.ParentSittingId,
                        principalSchema: "school",
                        principalTable: "exam_sittings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "exam_window_venues",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamScheduleWindowId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClassSectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CapacitySnapshot = table.Column<int>(type: "int", nullable: false),
                    VenueLabel = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_window_venues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_exam_window_venues_class_sections_ClassSectionId",
                        column: x => x.ClassSectionId,
                        principalSchema: "school",
                        principalTable: "class_sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_window_venues_exam_schedule_windows_ExamScheduleWindowId",
                        column: x => x.ExamScheduleWindowId,
                        principalSchema: "school",
                        principalTable: "exam_schedule_windows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_window_venues_school_rooms_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "school",
                        principalTable: "school_rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "exam_sitting_venues",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamSittingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamWindowVenueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_sitting_venues", x => x.Id);
                    table.UniqueConstraint("AK_exam_sitting_venues_Id_ExamSittingId", x => new { x.Id, x.ExamSittingId });
                    table.ForeignKey(
                        name: "FK_exam_sitting_venues_exam_sittings_ExamSittingId",
                        column: x => x.ExamSittingId,
                        principalSchema: "school",
                        principalTable: "exam_sittings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_sitting_venues_exam_window_venues_ExamWindowVenueId",
                        column: x => x.ExamWindowVenueId,
                        principalSchema: "school",
                        principalTable: "exam_window_venues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "exam_candidate_sitting_assignments",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamPaperId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamPaperCandidateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamSittingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamSittingVenueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeskOrSeatLabel = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    AssignedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_candidate_sitting_assignments", x => x.Id);
                    table.UniqueConstraint("AK_exam_candidate_sitting_assignments_Id_ExamPaperCandidateId", x => new { x.Id, x.ExamPaperCandidateId });
                    table.ForeignKey(
                        name: "FK_exam_candidate_sitting_assignments_exam_paper_candidates_ExamPaperCandidateId_ExamPaperId",
                        columns: x => new { x.ExamPaperCandidateId, x.ExamPaperId },
                        principalSchema: "school",
                        principalTable: "exam_paper_candidates",
                        principalColumns: new[] { "Id", "ExamPaperId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_candidate_sitting_assignments_exam_sitting_venues_ExamSittingVenueId_ExamSittingId",
                        columns: x => new { x.ExamSittingVenueId, x.ExamSittingId },
                        principalSchema: "school",
                        principalTable: "exam_sitting_venues",
                        principalColumns: new[] { "Id", "ExamSittingId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_candidate_sitting_assignments_exam_sittings_ExamSittingId_ExamPaperId",
                        columns: x => new { x.ExamSittingId, x.ExamPaperId },
                        principalSchema: "school",
                        principalTable: "exam_sittings",
                        principalColumns: new[] { "Id", "ExamPaperId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "exam_attendance",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamCandidateSittingAssignmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    ArrivedAt = table.Column<TimeOnly>(type: "time", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RecordedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RecordedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    FinalizedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FinalizedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_attendance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_exam_attendance_exam_candidate_sitting_assignments_ExamCandidateSittingAssignmentId",
                        column: x => x.ExamCandidateSittingAssignmentId,
                        principalSchema: "school",
                        principalTable: "exam_candidate_sitting_assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "exam_result_attempts",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamPaperCandidateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamCandidateSittingAssignmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AttemptNumber = table.Column<int>(type: "int", nullable: false),
                    Disposition = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    Score = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    IsFinal = table.Column<bool>(type: "bit", nullable: false),
                    MarkerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModeratorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MarkedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModeratedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ApprovedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    PublishedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_result_attempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_exam_result_attempts_exam_candidate_sitting_assignments_ExamCandidateSittingAssignmentId",
                        column: x => x.ExamCandidateSittingAssignmentId,
                        principalSchema: "school",
                        principalTable: "exam_candidate_sitting_assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_result_attempts_exam_paper_candidates_ExamPaperCandidateId",
                        column: x => x.ExamPaperCandidateId,
                        principalSchema: "school",
                        principalTable: "exam_paper_candidates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "school",
                table: "local_permissions",
                columns: new[] { "Id", "Code", "CreatedAtUtc", "DisplayNameAr", "DisplayNameEn", "IsActive", "Module", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd2c"), "school.exams.view", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "عرض الاختبارات", "View exams", true, "exams", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd2d"), "school.exams.manage", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "إدارة الاختبارات", "Manage exams", true, "exams", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd2e"), "school.exams.schedule", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "جدولة الاختبارات", "Schedule exams", true, "exams", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd2f"), "school.exams.approve", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "اعتماد الاختبارات", "Approve exams", true, "exams", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd30"), "school.exams.publish", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "نشر الاختبارات", "Publish exams", true, "exams", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd31"), "school.exams.attendance.record", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "تسجيل حضور الاختبارات", "Record exam attendance", true, "exams", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd32"), "school.exams.results.enter", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "رصد درجات الاختبارات", "Enter exam results", true, "exams", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd33"), "school.exams.results.approve", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "اعتماد نتائج الاختبارات", "Approve exam results", true, "exams", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd34"), "school.exams.results.publish", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "نشر نتائج الاختبارات", "Publish exam results", true, "exams", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd35"), "school.exams.results.reopen", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "إعادة فتح نتائج الاختبارات", "Reopen exam results", true, "exams", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd36"), "school.exams.cancel", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "إلغاء الاختبارات", "Cancel exams", true, "exams", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd37"), "school.exams.reports", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "تقارير الاختبارات", "Exam reports", true, "exams", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd38"), "school.exams.override_warnings", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "تجاوز تحذيرات الاختبارات", "Override exam warnings", true, "exams", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd39"), "school.exams.policy.manage", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "إدارة سياسات الاختبارات", "Manage exam policies", true, "exams", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd3a"), "school.exams.committees.manage", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "إدارة لجان الاختبارات", "Manage exam committees", true, "exams", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd3b"), "school.exams.seating.manage", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "إدارة أرقام الجلوس والمقاعد", "Manage exam seating", true, "exams", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd3c"), "school.exams.invigilators.manage", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "إدارة مراقبي الاختبارات", "Manage exam invigilators", true, "exams", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.InsertData(
                schema: "school",
                table: "local_role_permissions",
                columns: new[] { "PermissionId", "RoleId", "GrantedAtUtc", "GrantedByUserId" },
                values: new object[,]
                {
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd2c"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd2d"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd2e"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd2f"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd30"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd31"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd32"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd33"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd34"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd35"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd36"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd37"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd38"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd39"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd3a"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd3b"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd3c"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_exam_approvals_ExamPaperId",
                schema: "school",
                table: "exam_approvals",
                column: "ExamPaperId");

            migrationBuilder.CreateIndex(
                name: "IX_exam_approvals_ExamSeriesId_ExamPaperId_Stage_StepOrder",
                schema: "school",
                table: "exam_approvals",
                columns: new[] { "ExamSeriesId", "ExamPaperId", "Stage", "StepOrder" },
                unique: true,
                filter: "[ExamPaperId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_exam_attendance_ExamCandidateSittingAssignmentId",
                schema: "school",
                table: "exam_attendance",
                column: "ExamCandidateSittingAssignmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_audits_ActorUserId",
                schema: "school",
                table: "exam_audits",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_exam_audits_ExamSeriesId_CreatedAtUtc",
                schema: "school",
                table: "exam_audits",
                columns: new[] { "ExamSeriesId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_exam_calendar_projections_ExamScheduleWindowId",
                schema: "school",
                table: "exam_calendar_projections",
                column: "ExamScheduleWindowId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_calendar_projections_SchoolCalendarEventId",
                schema: "school",
                table: "exam_calendar_projections",
                column: "SchoolCalendarEventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_candidate_sitting_assignments_ExamPaperCandidateId_ExamPaperId",
                schema: "school",
                table: "exam_candidate_sitting_assignments",
                columns: new[] { "ExamPaperCandidateId", "ExamPaperId" });

            migrationBuilder.CreateIndex(
                name: "IX_exam_candidate_sitting_assignments_ExamSittingId_ExamPaperCandidateId",
                schema: "school",
                table: "exam_candidate_sitting_assignments",
                columns: new[] { "ExamSittingId", "ExamPaperCandidateId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_candidate_sitting_assignments_ExamSittingId_ExamPaperId",
                schema: "school",
                table: "exam_candidate_sitting_assignments",
                columns: new[] { "ExamSittingId", "ExamPaperId" });

            migrationBuilder.CreateIndex(
                name: "IX_exam_candidate_sitting_assignments_ExamSittingVenueId_ExamSittingId",
                schema: "school",
                table: "exam_candidate_sitting_assignments",
                columns: new[] { "ExamSittingVenueId", "ExamSittingId" });

            migrationBuilder.CreateIndex(
                name: "IX_exam_candidates_ExamSeriesId_StudentId",
                schema: "school",
                table: "exam_candidates",
                columns: new[] { "ExamSeriesId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_candidates_StudentEnrollmentIdSnapshot",
                schema: "school",
                table: "exam_candidates",
                column: "StudentEnrollmentIdSnapshot");

            migrationBuilder.CreateIndex(
                name: "IX_exam_candidates_StudentId",
                schema: "school",
                table: "exam_candidates",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_exam_paper_candidates_ExamCandidateId",
                schema: "school",
                table: "exam_paper_candidates",
                column: "ExamCandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_exam_paper_candidates_ExamPaperId_ExamCandidateId",
                schema: "school",
                table: "exam_paper_candidates",
                columns: new[] { "ExamPaperId", "ExamCandidateId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_paper_targets_ClassSectionId",
                schema: "school",
                table: "exam_paper_targets",
                column: "ClassSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_exam_paper_targets_ClassSectionSubjectId",
                schema: "school",
                table: "exam_paper_targets",
                column: "ClassSectionSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_exam_paper_targets_ExamPaperId_ClassSectionId_ClassSectionSubjectId",
                schema: "school",
                table: "exam_paper_targets",
                columns: new[] { "ExamPaperId", "ClassSectionId", "ClassSectionSubjectId" },
                unique: true,
                filter: "[ClassSectionId] IS NOT NULL AND [ClassSectionSubjectId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_exam_paper_targets_GradeOfferingId",
                schema: "school",
                table: "exam_paper_targets",
                column: "GradeOfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_exam_papers_ContentOwnerUserId",
                schema: "school",
                table: "exam_papers",
                column: "ContentOwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_exam_papers_ExamSeriesId_PaperCode",
                schema: "school",
                table: "exam_papers",
                columns: new[] { "ExamSeriesId", "PaperCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_papers_GradeSubjectOfferingId",
                schema: "school",
                table: "exam_papers",
                column: "GradeSubjectOfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_exam_policies_EducationProgramId_EducationStageId_ExamKind_Version",
                schema: "school",
                table: "exam_policies",
                columns: new[] { "EducationProgramId", "EducationStageId", "ExamKind", "Version" },
                unique: true,
                filter: "[EducationProgramId] IS NOT NULL AND [EducationStageId] IS NOT NULL AND [ExamKind] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_exam_policies_EducationStageId",
                schema: "school",
                table: "exam_policies",
                column: "EducationStageId");

            migrationBuilder.CreateIndex(
                name: "IX_exam_result_attempts_ExamCandidateSittingAssignmentId",
                schema: "school",
                table: "exam_result_attempts",
                column: "ExamCandidateSittingAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_exam_result_attempts_ExamPaperCandidateId",
                schema: "school",
                table: "exam_result_attempts",
                column: "ExamPaperCandidateId",
                unique: true,
                filter: "[IsFinal] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_exam_result_attempts_ExamPaperCandidateId_AttemptNumber",
                schema: "school",
                table: "exam_result_attempts",
                columns: new[] { "ExamPaperCandidateId", "AttemptNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_schedule_windows_ExamSeriesId",
                schema: "school",
                table: "exam_schedule_windows",
                column: "ExamSeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_exam_schedule_windows_LocalDate_StartsAtUtc_EndsAtUtc_Status",
                schema: "school",
                table: "exam_schedule_windows",
                columns: new[] { "LocalDate", "StartsAtUtc", "EndsAtUtc", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_exam_schedule_windows_RescheduledFromWindowId",
                schema: "school",
                table: "exam_schedule_windows",
                column: "RescheduledFromWindowId");

            migrationBuilder.CreateIndex(
                name: "IX_exam_series_AcademicTermId",
                schema: "school",
                table: "exam_series",
                column: "AcademicTermId");

            migrationBuilder.CreateIndex(
                name: "IX_exam_series_CreatedByUserId",
                schema: "school",
                table: "exam_series",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_exam_series_ExternalSourceCode_ExternalReferenceId_ExternalRevision",
                schema: "school",
                table: "exam_series",
                columns: new[] { "ExternalSourceCode", "ExternalReferenceId", "ExternalRevision" },
                unique: true,
                filter: "[ExternalSourceCode] IS NOT NULL AND [ExternalReferenceId] IS NOT NULL AND [ExternalRevision] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_exam_series_ProgramAcademicYearId_Code",
                schema: "school",
                table: "exam_series",
                columns: new[] { "ProgramAcademicYearId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_series_Status_Kind_CreatedAtUtc",
                schema: "school",
                table: "exam_series",
                columns: new[] { "Status", "Kind", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_exam_series_targets_ClassSectionId",
                schema: "school",
                table: "exam_series_targets",
                column: "ClassSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_exam_series_targets_ExamSeriesId_GradeOfferingId_ClassSectionId",
                schema: "school",
                table: "exam_series_targets",
                columns: new[] { "ExamSeriesId", "GradeOfferingId", "ClassSectionId" },
                unique: true,
                filter: "[ClassSectionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_exam_series_targets_GradeOfferingId",
                schema: "school",
                table: "exam_series_targets",
                column: "GradeOfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_exam_sitting_venues_ExamSittingId_ExamWindowVenueId",
                schema: "school",
                table: "exam_sitting_venues",
                columns: new[] { "ExamSittingId", "ExamWindowVenueId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_sitting_venues_ExamWindowVenueId",
                schema: "school",
                table: "exam_sitting_venues",
                column: "ExamWindowVenueId");

            migrationBuilder.CreateIndex(
                name: "IX_exam_sittings_ExamPaperId",
                schema: "school",
                table: "exam_sittings",
                column: "ExamPaperId");

            migrationBuilder.CreateIndex(
                name: "IX_exam_sittings_ExamScheduleWindowId_ExamPaperId",
                schema: "school",
                table: "exam_sittings",
                columns: new[] { "ExamScheduleWindowId", "ExamPaperId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_sittings_ParentSittingId",
                schema: "school",
                table: "exam_sittings",
                column: "ParentSittingId");

            migrationBuilder.CreateIndex(
                name: "IX_exam_window_venues_ClassSectionId",
                schema: "school",
                table: "exam_window_venues",
                column: "ClassSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_exam_window_venues_ExamScheduleWindowId_ClassSectionId",
                schema: "school",
                table: "exam_window_venues",
                columns: new[] { "ExamScheduleWindowId", "ClassSectionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_window_venues_RoomId",
                schema: "school",
                table: "exam_window_venues",
                column: "RoomId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exam_approvals",
                schema: "school");

            migrationBuilder.DropTable(
                name: "exam_attendance",
                schema: "school");

            migrationBuilder.DropTable(
                name: "exam_audits",
                schema: "school");

            migrationBuilder.DropTable(
                name: "exam_calendar_projections",
                schema: "school");

            migrationBuilder.DropTable(
                name: "exam_paper_targets",
                schema: "school");

            migrationBuilder.DropTable(
                name: "exam_policies",
                schema: "school");

            migrationBuilder.DropTable(
                name: "exam_result_attempts",
                schema: "school");

            migrationBuilder.DropTable(
                name: "exam_series_targets",
                schema: "school");

            migrationBuilder.DropTable(
                name: "exam_candidate_sitting_assignments",
                schema: "school");

            migrationBuilder.DropTable(
                name: "exam_paper_candidates",
                schema: "school");

            migrationBuilder.DropTable(
                name: "exam_sitting_venues",
                schema: "school");

            migrationBuilder.DropTable(
                name: "exam_candidates",
                schema: "school");

            migrationBuilder.DropTable(
                name: "exam_sittings",
                schema: "school");

            migrationBuilder.DropTable(
                name: "exam_window_venues",
                schema: "school");

            migrationBuilder.DropTable(
                name: "exam_papers",
                schema: "school");

            migrationBuilder.DropTable(
                name: "exam_schedule_windows",
                schema: "school");

            migrationBuilder.DropTable(
                name: "exam_series",
                schema: "school");

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd2c"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd2d"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd2e"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd2f"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd30"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd31"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd32"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd33"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd34"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd35"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd36"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd37"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd38"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd39"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd3a"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd3b"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd3c"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd2c"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd2d"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd2e"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd2f"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd30"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd31"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd32"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd33"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd34"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd35"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd36"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd37"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd38"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd39"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd3a"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd3b"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd3c"));
        }
    }
}
