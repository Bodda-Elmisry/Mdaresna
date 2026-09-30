using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Schools.Infrastructure.Persistence.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolExamOperations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_exam_window_venues_ExamScheduleWindowId_ClassSectionId",
                schema: "school",
                table: "exam_window_venues");

            migrationBuilder.AlterColumn<Guid>(
                name: "ClassSectionId",
                schema: "school",
                table: "exam_window_venues",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "ExamCommitteeId",
                schema: "school",
                table: "exam_window_venues",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExamNumber",
                schema: "school",
                table: "exam_candidates",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SeatNumber",
                schema: "school",
                table: "exam_candidate_sitting_assignments",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "exam_committees",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamSeriesId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamScheduleWindowId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_committees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_exam_committees_exam_schedule_windows_ExamScheduleWindowId",
                        column: x => x.ExamScheduleWindowId,
                        principalSchema: "school",
                        principalTable: "exam_schedule_windows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_committees_exam_series_ExamSeriesId",
                        column: x => x.ExamSeriesId,
                        principalSchema: "school",
                        principalTable: "exam_series",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "exam_result_appeals",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamPaperCandidateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    DecisionNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PreviousScore = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    RevisedScore = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReviewedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_result_appeals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_exam_result_appeals_exam_paper_candidates_ExamPaperCandidat~",
                        column: x => x.ExamPaperCandidateId,
                        principalSchema: "school",
                        principalTable: "exam_paper_candidates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "exam_invigilator_assignments",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamCommitteeId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    ReplacesAssignmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AssignedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_invigilator_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_exam_invigilator_assignments_exam_committees_ExamCommitteeId",
                        column: x => x.ExamCommitteeId,
                        principalSchema: "school",
                        principalTable: "exam_committees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_invigilator_assignments_exam_invigilator_assignments_R~",
                        column: x => x.ReplacesAssignmentId,
                        principalSchema: "school",
                        principalTable: "exam_invigilator_assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_invigilator_assignments_local_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "school",
                        principalTable: "local_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_exam_window_venues_ExamCommitteeId",
                schema: "school",
                table: "exam_window_venues",
                column: "ExamCommitteeId");

            migrationBuilder.CreateIndex(
                name: "IX_exam_window_venues_ExamScheduleWindowId_ClassSectionId",
                schema: "school",
                table: "exam_window_venues",
                columns: new[] { "ExamScheduleWindowId", "ClassSectionId" });

            migrationBuilder.CreateIndex(
                name: "IX_exam_window_venues_ExamScheduleWindowId_ExamCommitteeId",
                schema: "school",
                table: "exam_window_venues",
                columns: new[] { "ExamScheduleWindowId", "ExamCommitteeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_candidates_ExamSeriesId_ExamNumber",
                schema: "school",
                table: "exam_candidates",
                columns: new[] { "ExamSeriesId", "ExamNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_exam_committees_ExamScheduleWindowId_SortOrder",
                schema: "school",
                table: "exam_committees",
                columns: new[] { "ExamScheduleWindowId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_committees_ExamSeriesId_Code",
                schema: "school",
                table: "exam_committees",
                columns: new[] { "ExamSeriesId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_invigilator_assignments_ExamCommitteeId_UserId",
                schema: "school",
                table: "exam_invigilator_assignments",
                columns: new[] { "ExamCommitteeId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_invigilator_assignments_ReplacesAssignmentId",
                schema: "school",
                table: "exam_invigilator_assignments",
                column: "ReplacesAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_exam_invigilator_assignments_UserId_Status",
                schema: "school",
                table: "exam_invigilator_assignments",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_exam_result_appeals_ExamPaperCandidateId_Status",
                schema: "school",
                table: "exam_result_appeals",
                columns: new[] { "ExamPaperCandidateId", "Status" });

            migrationBuilder.AddForeignKey(
                name: "FK_exam_window_venues_exam_committees_ExamCommitteeId",
                schema: "school",
                table: "exam_window_venues",
                column: "ExamCommitteeId",
                principalSchema: "school",
                principalTable: "exam_committees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_exam_window_venues_exam_committees_ExamCommitteeId",
                schema: "school",
                table: "exam_window_venues");

            migrationBuilder.DropTable(
                name: "exam_invigilator_assignments",
                schema: "school");

            migrationBuilder.DropTable(
                name: "exam_result_appeals",
                schema: "school");

            migrationBuilder.DropTable(
                name: "exam_committees",
                schema: "school");

            migrationBuilder.DropIndex(
                name: "IX_exam_window_venues_ExamCommitteeId",
                schema: "school",
                table: "exam_window_venues");

            migrationBuilder.DropIndex(
                name: "IX_exam_window_venues_ExamScheduleWindowId_ClassSectionId",
                schema: "school",
                table: "exam_window_venues");

            migrationBuilder.DropIndex(
                name: "IX_exam_window_venues_ExamScheduleWindowId_ExamCommitteeId",
                schema: "school",
                table: "exam_window_venues");

            migrationBuilder.DropIndex(
                name: "IX_exam_candidates_ExamSeriesId_ExamNumber",
                schema: "school",
                table: "exam_candidates");

            migrationBuilder.DropColumn(
                name: "ExamCommitteeId",
                schema: "school",
                table: "exam_window_venues");

            migrationBuilder.DropColumn(
                name: "ExamNumber",
                schema: "school",
                table: "exam_candidates");

            migrationBuilder.DropColumn(
                name: "SeatNumber",
                schema: "school",
                table: "exam_candidate_sitting_assignments");

            migrationBuilder.AlterColumn<Guid>(
                name: "ClassSectionId",
                schema: "school",
                table: "exam_window_venues",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_window_venues_ExamScheduleWindowId_ClassSectionId",
                schema: "school",
                table: "exam_window_venues",
                columns: new[] { "ExamScheduleWindowId", "ClassSectionId" },
                unique: true);
        }
    }
}
