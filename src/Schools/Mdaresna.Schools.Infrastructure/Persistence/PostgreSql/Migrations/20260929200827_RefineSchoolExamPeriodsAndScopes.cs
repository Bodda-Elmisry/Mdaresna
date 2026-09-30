using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Schools.Infrastructure.Persistence.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class RefineSchoolExamPeriodsAndScopes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "AssessmentMonth",
                schema: "school",
                table: "exam_series",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EducationStageId",
                schema: "school",
                table: "exam_series",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentExamSeriesId",
                schema: "school",
                table: "exam_series",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Purpose",
                schema: "school",
                table: "exam_series",
                type: "character varying(24)",
                maxLength: 24,
                nullable: false,
                defaultValue: "Regular");

            migrationBuilder.AddColumn<Guid>(
                name: "ScopeClassSectionId",
                schema: "school",
                table: "exam_series",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ScopeGradeOfferingId",
                schema: "school",
                table: "exam_series",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ScopeLevel",
                schema: "school",
                table: "exam_series",
                type: "character varying(24)",
                maxLength: 24,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_series_EducationStageId",
                schema: "school",
                table: "exam_series",
                column: "EducationStageId");

            migrationBuilder.CreateIndex(
                name: "IX_exam_series_ParentExamSeriesId",
                schema: "school",
                table: "exam_series",
                column: "ParentExamSeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_exam_series_ProgramAcademicYearId_AcademicTermId_Assessment~",
                schema: "school",
                table: "exam_series",
                columns: new[] { "ProgramAcademicYearId", "AcademicTermId", "AssessmentMonth", "Kind" });

            migrationBuilder.CreateIndex(
                name: "IX_exam_series_ScopeClassSectionId",
                schema: "school",
                table: "exam_series",
                column: "ScopeClassSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_exam_series_ScopeGradeOfferingId",
                schema: "school",
                table: "exam_series",
                column: "ScopeGradeOfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_exam_series_ScopeLevel_EducationStageId_ScopeGradeOfferingI~",
                schema: "school",
                table: "exam_series",
                columns: new[] { "ScopeLevel", "EducationStageId", "ScopeGradeOfferingId", "ScopeClassSectionId" });

            migrationBuilder.AddForeignKey(
                name: "FK_exam_series_class_sections_ScopeClassSectionId",
                schema: "school",
                table: "exam_series",
                column: "ScopeClassSectionId",
                principalSchema: "school",
                principalTable: "class_sections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_exam_series_education_stages_EducationStageId",
                schema: "school",
                table: "exam_series",
                column: "EducationStageId",
                principalSchema: "school",
                principalTable: "education_stages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_exam_series_exam_series_ParentExamSeriesId",
                schema: "school",
                table: "exam_series",
                column: "ParentExamSeriesId",
                principalSchema: "school",
                principalTable: "exam_series",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_exam_series_grade_offerings_ScopeGradeOfferingId",
                schema: "school",
                table: "exam_series",
                column: "ScopeGradeOfferingId",
                principalSchema: "school",
                principalTable: "grade_offerings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_exam_series_class_sections_ScopeClassSectionId",
                schema: "school",
                table: "exam_series");

            migrationBuilder.DropForeignKey(
                name: "FK_exam_series_education_stages_EducationStageId",
                schema: "school",
                table: "exam_series");

            migrationBuilder.DropForeignKey(
                name: "FK_exam_series_exam_series_ParentExamSeriesId",
                schema: "school",
                table: "exam_series");

            migrationBuilder.DropForeignKey(
                name: "FK_exam_series_grade_offerings_ScopeGradeOfferingId",
                schema: "school",
                table: "exam_series");

            migrationBuilder.DropIndex(
                name: "IX_exam_series_EducationStageId",
                schema: "school",
                table: "exam_series");

            migrationBuilder.DropIndex(
                name: "IX_exam_series_ParentExamSeriesId",
                schema: "school",
                table: "exam_series");

            migrationBuilder.DropIndex(
                name: "IX_exam_series_ProgramAcademicYearId_AcademicTermId_Assessment~",
                schema: "school",
                table: "exam_series");

            migrationBuilder.DropIndex(
                name: "IX_exam_series_ScopeClassSectionId",
                schema: "school",
                table: "exam_series");

            migrationBuilder.DropIndex(
                name: "IX_exam_series_ScopeGradeOfferingId",
                schema: "school",
                table: "exam_series");

            migrationBuilder.DropIndex(
                name: "IX_exam_series_ScopeLevel_EducationStageId_ScopeGradeOfferingI~",
                schema: "school",
                table: "exam_series");

            migrationBuilder.DropColumn(
                name: "AssessmentMonth",
                schema: "school",
                table: "exam_series");

            migrationBuilder.DropColumn(
                name: "EducationStageId",
                schema: "school",
                table: "exam_series");

            migrationBuilder.DropColumn(
                name: "ParentExamSeriesId",
                schema: "school",
                table: "exam_series");

            migrationBuilder.DropColumn(
                name: "Purpose",
                schema: "school",
                table: "exam_series");

            migrationBuilder.DropColumn(
                name: "ScopeClassSectionId",
                schema: "school",
                table: "exam_series");

            migrationBuilder.DropColumn(
                name: "ScopeGradeOfferingId",
                schema: "school",
                table: "exam_series");

            migrationBuilder.DropColumn(
                name: "ScopeLevel",
                schema: "school",
                table: "exam_series");
        }
    }
}
