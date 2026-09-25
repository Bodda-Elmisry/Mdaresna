using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Schools.Infrastructure.Persistence.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolTeachingPlanning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "class_section_subjects",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassSectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GradeSubjectOfferingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_class_section_subjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_class_section_subjects_class_sections_ClassSectionId",
                        column: x => x.ClassSectionId,
                        principalSchema: "school",
                        principalTable: "class_sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_class_section_subjects_grade_subject_offerings_GradeSubjectOfferingId",
                        column: x => x.GradeSubjectOfferingId,
                        principalSchema: "school",
                        principalTable: "grade_subject_offerings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "teacher_grade_subject_scopes",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeacherUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GradeSubjectOfferingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teacher_grade_subject_scopes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_teacher_grade_subject_scopes_grade_subject_offerings_GradeSubjectOfferingId",
                        column: x => x.GradeSubjectOfferingId,
                        principalSchema: "school",
                        principalTable: "grade_subject_offerings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_teacher_grade_subject_scopes_local_users_TeacherUserId",
                        column: x => x.TeacherUserId,
                        principalSchema: "school",
                        principalTable: "local_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "weekly_timetable_slots",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassSectionSubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DayOfWeek = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    StartsAt = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndsAt = table.Column<TimeOnly>(type: "time", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_weekly_timetable_slots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_weekly_timetable_slots_class_section_subjects_ClassSectionSubjectId",
                        column: x => x.ClassSectionSubjectId,
                        principalSchema: "school",
                        principalTable: "class_section_subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "class_subject_teacher_assignments",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassSectionSubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeacherGradeSubjectScopeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_class_subject_teacher_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_class_subject_teacher_assignments_class_section_subjects_ClassSectionSubjectId",
                        column: x => x.ClassSectionSubjectId,
                        principalSchema: "school",
                        principalTable: "class_section_subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_class_subject_teacher_assignments_teacher_grade_subject_scopes_TeacherGradeSubjectScopeId",
                        column: x => x.TeacherGradeSubjectScopeId,
                        principalSchema: "school",
                        principalTable: "teacher_grade_subject_scopes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "teacher_substitutions",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WeeklyTimetableSlotId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubstituteTeacherAssignmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LessonDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teacher_substitutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_teacher_substitutions_class_subject_teacher_assignments_SubstituteTeacherAssignmentId",
                        column: x => x.SubstituteTeacherAssignmentId,
                        principalSchema: "school",
                        principalTable: "class_subject_teacher_assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_teacher_substitutions_weekly_timetable_slots_WeeklyTimetableSlotId",
                        column: x => x.WeeklyTimetableSlotId,
                        principalSchema: "school",
                        principalTable: "weekly_timetable_slots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_class_section_subjects_ClassSectionId_GradeSubjectOfferingId",
                schema: "school",
                table: "class_section_subjects",
                columns: new[] { "ClassSectionId", "GradeSubjectOfferingId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_class_section_subjects_GradeSubjectOfferingId",
                schema: "school",
                table: "class_section_subjects",
                column: "GradeSubjectOfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_class_subject_teacher_assignments_ClassSectionSubjectId_Role_TeacherGradeSubjectScopeId",
                schema: "school",
                table: "class_subject_teacher_assignments",
                columns: new[] { "ClassSectionSubjectId", "Role", "TeacherGradeSubjectScopeId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_class_subject_teacher_assignments_TeacherGradeSubjectScopeId",
                schema: "school",
                table: "class_subject_teacher_assignments",
                column: "TeacherGradeSubjectScopeId");

            migrationBuilder.CreateIndex(
                name: "IX_teacher_grade_subject_scopes_GradeSubjectOfferingId",
                schema: "school",
                table: "teacher_grade_subject_scopes",
                column: "GradeSubjectOfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_teacher_grade_subject_scopes_TeacherUserId_GradeSubjectOfferingId",
                schema: "school",
                table: "teacher_grade_subject_scopes",
                columns: new[] { "TeacherUserId", "GradeSubjectOfferingId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_teacher_substitutions_SubstituteTeacherAssignmentId",
                schema: "school",
                table: "teacher_substitutions",
                column: "SubstituteTeacherAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_teacher_substitutions_WeeklyTimetableSlotId_LessonDate",
                schema: "school",
                table: "teacher_substitutions",
                columns: new[] { "WeeklyTimetableSlotId", "LessonDate" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_weekly_timetable_slots_ClassSectionSubjectId_DayOfWeek_StartsAt_EndsAt",
                schema: "school",
                table: "weekly_timetable_slots",
                columns: new[] { "ClassSectionSubjectId", "DayOfWeek", "StartsAt", "EndsAt" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "teacher_substitutions",
                schema: "school");

            migrationBuilder.DropTable(
                name: "class_subject_teacher_assignments",
                schema: "school");

            migrationBuilder.DropTable(
                name: "weekly_timetable_slots",
                schema: "school");

            migrationBuilder.DropTable(
                name: "teacher_grade_subject_scopes",
                schema: "school");

            migrationBuilder.DropTable(
                name: "class_section_subjects",
                schema: "school");
        }
    }
}
