using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Schools.Infrastructure.Persistence.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class RedesignWeeklyTimetableMatrix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_weekly_timetable_slots_ClassSectionSubjectId_DayOfWeek_StartsAt_EndsAt",
                schema: "school",
                table: "weekly_timetable_slots");

            migrationBuilder.AlterColumn<Guid>(
                name: "ClassSectionSubjectId",
                schema: "school",
                table: "weekly_timetable_slots",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "ClassSectionId",
                schema: "school",
                table: "weekly_timetable_slots",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsBreak",
                schema: "school",
                table: "weekly_timetable_slots",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "PrimaryTeacherScopeId",
                schema: "school",
                table: "weekly_timetable_slots",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SlotNumber",
                schema: "school",
                table: "weekly_timetable_slots",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DailyBreakCount",
                schema: "school",
                table: "education_stages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DailyLessonCount",
                schema: "school",
                table: "education_stages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "weekly_timetable_slot_substitute_teachers",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WeeklyTimetableSlotId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeacherGradeSubjectScopeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_weekly_timetable_slot_substitute_teachers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_weekly_timetable_slot_substitute_teachers_teacher_grade_subject_scopes_TeacherGradeSubjectScopeId",
                        column: x => x.TeacherGradeSubjectScopeId,
                        principalSchema: "school",
                        principalTable: "teacher_grade_subject_scopes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_weekly_timetable_slot_substitute_teachers_weekly_timetable_slots_WeeklyTimetableSlotId",
                        column: x => x.WeeklyTimetableSlotId,
                        principalSchema: "school",
                        principalTable: "weekly_timetable_slots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql("""
                UPDATE slot
                SET slot.ClassSectionId = subject.ClassSectionId,
                    slot.PrimaryTeacherScopeId = primary_assignment.TeacherGradeSubjectScopeId
                FROM school.weekly_timetable_slots AS slot
                INNER JOIN school.class_section_subjects AS subject
                    ON subject.Id = slot.ClassSectionSubjectId
                OUTER APPLY (
                    SELECT TOP (1) assignment.TeacherGradeSubjectScopeId
                    FROM school.class_subject_teacher_assignments AS assignment
                    WHERE assignment.ClassSectionSubjectId = subject.Id
                      AND assignment.Role = 'Primary'
                      AND assignment.IsActive = 1
                      AND assignment.IsDeleted = 0
                    ORDER BY assignment.CreatedAtUtc, assignment.Id
                ) AS primary_assignment;

                WITH numbered AS (
                    SELECT Id, ROW_NUMBER() OVER (
                        PARTITION BY ClassSectionId, DayOfWeek
                        ORDER BY StartsAt, EndsAt, Id) AS SlotNumber
                    FROM school.weekly_timetable_slots
                )
                UPDATE slot
                SET slot.SlotNumber = numbered.SlotNumber
                FROM school.weekly_timetable_slots AS slot
                INNER JOIN numbered ON numbered.Id = slot.Id;

                INSERT INTO school.weekly_timetable_slot_substitute_teachers
                    (Id, WeeklyTimetableSlotId, TeacherGradeSubjectScopeId, IsActive, IsDeleted, CreatedAtUtc, UpdatedAtUtc)
                SELECT NEWID(), slot.Id, assignment.TeacherGradeSubjectScopeId, 1, 0,
                       slot.CreatedAtUtc, slot.UpdatedAtUtc
                FROM school.weekly_timetable_slots AS slot
                INNER JOIN school.class_subject_teacher_assignments AS assignment
                    ON assignment.ClassSectionSubjectId = slot.ClassSectionSubjectId
                   AND assignment.Role = 'Substitute'
                   AND assignment.IsActive = 1
                   AND assignment.IsDeleted = 0;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_weekly_timetable_slots_ClassSectionId_DayOfWeek_SlotNumber",
                schema: "school",
                table: "weekly_timetable_slots",
                columns: new[] { "ClassSectionId", "DayOfWeek", "SlotNumber" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_weekly_timetable_slots_ClassSectionSubjectId",
                schema: "school",
                table: "weekly_timetable_slots",
                column: "ClassSectionSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_weekly_timetable_slots_PrimaryTeacherScopeId",
                schema: "school",
                table: "weekly_timetable_slots",
                column: "PrimaryTeacherScopeId");

            migrationBuilder.CreateIndex(
                name: "IX_weekly_timetable_slot_substitute_teachers_TeacherGradeSubjectScopeId",
                schema: "school",
                table: "weekly_timetable_slot_substitute_teachers",
                column: "TeacherGradeSubjectScopeId");

            migrationBuilder.CreateIndex(
                name: "IX_weekly_timetable_slot_substitute_teachers_WeeklyTimetableSlotId_TeacherGradeSubjectScopeId",
                schema: "school",
                table: "weekly_timetable_slot_substitute_teachers",
                columns: new[] { "WeeklyTimetableSlotId", "TeacherGradeSubjectScopeId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_weekly_timetable_slots_class_sections_ClassSectionId",
                schema: "school",
                table: "weekly_timetable_slots",
                column: "ClassSectionId",
                principalSchema: "school",
                principalTable: "class_sections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_weekly_timetable_slots_teacher_grade_subject_scopes_PrimaryTeacherScopeId",
                schema: "school",
                table: "weekly_timetable_slots",
                column: "PrimaryTeacherScopeId",
                principalSchema: "school",
                principalTable: "teacher_grade_subject_scopes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM school.weekly_timetable_slots WHERE IsBreak = 1 OR ClassSectionSubjectId IS NULL;");

            migrationBuilder.DropForeignKey(
                name: "FK_weekly_timetable_slots_class_sections_ClassSectionId",
                schema: "school",
                table: "weekly_timetable_slots");

            migrationBuilder.DropForeignKey(
                name: "FK_weekly_timetable_slots_teacher_grade_subject_scopes_PrimaryTeacherScopeId",
                schema: "school",
                table: "weekly_timetable_slots");

            migrationBuilder.DropTable(
                name: "weekly_timetable_slot_substitute_teachers",
                schema: "school");

            migrationBuilder.DropIndex(
                name: "IX_weekly_timetable_slots_ClassSectionId_DayOfWeek_SlotNumber",
                schema: "school",
                table: "weekly_timetable_slots");

            migrationBuilder.DropIndex(
                name: "IX_weekly_timetable_slots_ClassSectionSubjectId",
                schema: "school",
                table: "weekly_timetable_slots");

            migrationBuilder.DropIndex(
                name: "IX_weekly_timetable_slots_PrimaryTeacherScopeId",
                schema: "school",
                table: "weekly_timetable_slots");

            migrationBuilder.DropColumn(
                name: "ClassSectionId",
                schema: "school",
                table: "weekly_timetable_slots");

            migrationBuilder.DropColumn(
                name: "IsBreak",
                schema: "school",
                table: "weekly_timetable_slots");

            migrationBuilder.DropColumn(
                name: "PrimaryTeacherScopeId",
                schema: "school",
                table: "weekly_timetable_slots");

            migrationBuilder.DropColumn(
                name: "SlotNumber",
                schema: "school",
                table: "weekly_timetable_slots");

            migrationBuilder.DropColumn(
                name: "DailyBreakCount",
                schema: "school",
                table: "education_stages");

            migrationBuilder.DropColumn(
                name: "DailyLessonCount",
                schema: "school",
                table: "education_stages");

            migrationBuilder.AlterColumn<Guid>(
                name: "ClassSectionSubjectId",
                schema: "school",
                table: "weekly_timetable_slots",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_weekly_timetable_slots_ClassSectionSubjectId_DayOfWeek_StartsAt_EndsAt",
                schema: "school",
                table: "weekly_timetable_slots",
                columns: new[] { "ClassSectionSubjectId", "DayOfWeek", "StartsAt", "EndsAt" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }
    }
}
