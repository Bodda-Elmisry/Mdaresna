using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Schools.Infrastructure.Persistence.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddClassSectionTeacherScopes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "class_section_teacher_scopes",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassSectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeacherGradeSubjectScopeId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_class_section_teacher_scopes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_class_section_teacher_scopes_class_sections_ClassSectionId",
                        column: x => x.ClassSectionId,
                        principalSchema: "school",
                        principalTable: "class_sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_class_section_teacher_scopes_teacher_grade_subject_scopes_T~",
                        column: x => x.TeacherGradeSubjectScopeId,
                        principalSchema: "school",
                        principalTable: "teacher_grade_subject_scopes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql("""
                INSERT INTO school.class_section_teacher_scopes
                    ("Id", "ClassSectionId", "TeacherGradeSubjectScopeId", "IsActive", "IsDeleted",
                     "DeletedAtUtc", "DeletedByUserId", "CreatedAtUtc", "UpdatedAtUtc")
                SELECT DISTINCT ON (css."ClassSectionId", assignment."TeacherGradeSubjectScopeId")
                    assignment."Id", css."ClassSectionId", assignment."TeacherGradeSubjectScopeId", TRUE, FALSE,
                    NULL, NULL, assignment."CreatedAtUtc", assignment."UpdatedAtUtc"
                FROM school.class_subject_teacher_assignments AS assignment
                INNER JOIN school.class_section_subjects AS css ON css."Id" = assignment."ClassSectionSubjectId"
                WHERE assignment."IsActive" = TRUE AND assignment."IsDeleted" = FALSE
                  AND css."IsActive" = TRUE AND css."IsDeleted" = FALSE
                ORDER BY css."ClassSectionId", assignment."TeacherGradeSubjectScopeId", assignment."CreatedAtUtc";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_class_section_teacher_scopes_ClassSectionId_TeacherGradeSub~",
                schema: "school",
                table: "class_section_teacher_scopes",
                columns: new[] { "ClassSectionId", "TeacherGradeSubjectScopeId" },
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_class_section_teacher_scopes_TeacherGradeSubjectScopeId",
                schema: "school",
                table: "class_section_teacher_scopes",
                column: "TeacherGradeSubjectScopeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "class_section_teacher_scopes",
                schema: "school");
        }
    }
}
