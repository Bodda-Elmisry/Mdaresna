using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Schools.Infrastructure.Persistence.SqlServer.Migrations
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
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassSectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_class_section_teacher_scopes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_class_section_teacher_scopes_class_sections_ClassSectionId",
                        column: x => x.ClassSectionId,
                        principalSchema: "school",
                        principalTable: "class_sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_class_section_teacher_scopes_teacher_grade_subject_scopes_TeacherGradeSubjectScopeId",
                        column: x => x.TeacherGradeSubjectScopeId,
                        principalSchema: "school",
                        principalTable: "teacher_grade_subject_scopes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql("""
                WITH ExistingAssignments AS
                (
                    SELECT css.[ClassSectionId], assignment.[TeacherGradeSubjectScopeId],
                           assignment.[CreatedAtUtc], assignment.[UpdatedAtUtc],
                           ROW_NUMBER() OVER
                           (
                               PARTITION BY css.[ClassSectionId], assignment.[TeacherGradeSubjectScopeId]
                               ORDER BY assignment.[CreatedAtUtc]
                           ) AS [RowNumber]
                    FROM [school].[class_subject_teacher_assignments] AS assignment
                    INNER JOIN [school].[class_section_subjects] AS css
                        ON css.[Id] = assignment.[ClassSectionSubjectId]
                    WHERE assignment.[IsActive] = 1 AND assignment.[IsDeleted] = 0
                      AND css.[IsActive] = 1 AND css.[IsDeleted] = 0
                )
                INSERT INTO [school].[class_section_teacher_scopes]
                    ([Id], [ClassSectionId], [TeacherGradeSubjectScopeId], [IsActive], [IsDeleted],
                     [DeletedAtUtc], [DeletedByUserId], [CreatedAtUtc], [UpdatedAtUtc])
                SELECT NEWID(), [ClassSectionId], [TeacherGradeSubjectScopeId], 1, 0,
                       NULL, NULL, [CreatedAtUtc], [UpdatedAtUtc]
                FROM ExistingAssignments
                WHERE [RowNumber] = 1;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_class_section_teacher_scopes_ClassSectionId_TeacherGradeSubjectScopeId",
                schema: "school",
                table: "class_section_teacher_scopes",
                columns: new[] { "ClassSectionId", "TeacherGradeSubjectScopeId" },
                unique: true,
                filter: "[IsDeleted] = 0");

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
