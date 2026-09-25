using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Schools.Infrastructure.Persistence.PostgreSql.Migrations
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassSectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    GradeSubjectOfferingId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
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
                        name: "FK_class_section_subjects_grade_subject_offerings_GradeSubject~",
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeacherUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    GradeSubjectOfferingId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teacher_grade_subject_scopes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_teacher_grade_subject_scopes_grade_subject_offerings_GradeS~",
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassSectionSubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    StartsAt = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndsAt = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_weekly_timetable_slots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_weekly_timetable_slots_class_section_subjects_ClassSectionS~",
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassSectionSubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeacherGradeSubjectScopeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_class_subject_teacher_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_class_subject_teacher_assignments_class_section_subjects_Cl~",
                        column: x => x.ClassSectionSubjectId,
                        principalSchema: "school",
                        principalTable: "class_section_subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_class_subject_teacher_assignments_teacher_grade_subject_sco~",
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WeeklyTimetableSlotId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubstituteTeacherAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    LessonDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teacher_substitutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_teacher_substitutions_class_subject_teacher_assignments_Sub~",
                        column: x => x.SubstituteTeacherAssignmentId,
                        principalSchema: "school",
                        principalTable: "class_subject_teacher_assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_teacher_substitutions_weekly_timetable_slots_WeeklyTimetabl~",
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
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_class_section_subjects_GradeSubjectOfferingId",
                schema: "school",
                table: "class_section_subjects",
                column: "GradeSubjectOfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_class_subject_teacher_assignments_ClassSectionSubjectId_Rol~",
                schema: "school",
                table: "class_subject_teacher_assignments",
                columns: new[] { "ClassSectionSubjectId", "Role", "TeacherGradeSubjectScopeId" },
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

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
                name: "IX_teacher_grade_subject_scopes_TeacherUserId_GradeSubjectOffe~",
                schema: "school",
                table: "teacher_grade_subject_scopes",
                columns: new[] { "TeacherUserId", "GradeSubjectOfferingId" },
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

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
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_weekly_timetable_slots_ClassSectionSubjectId_DayOfWeek_Star~",
                schema: "school",
                table: "weekly_timetable_slots",
                columns: new[] { "ClassSectionSubjectId", "DayOfWeek", "StartsAt", "EndsAt" },
                unique: true,
                filter: "\"IsDeleted\" = FALSE");
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
