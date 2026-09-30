using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Schools.Infrastructure.Persistence.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddTeachingPlansAndDocumentCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "documents",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "teaching_plans",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    ProgramAcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GradeOfferingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GradeSubjectOfferingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicTermId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ParentPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TitleAr = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    TitleEn = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false),
                    SourceAuthority = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    SourceReference = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FromDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ToDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Revision = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmittedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PublishedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SubmittedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ApprovedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    PublishedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teaching_plans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_teaching_plans_academic_terms_AcademicTermId",
                        column: x => x.AcademicTermId,
                        principalSchema: "school",
                        principalTable: "academic_terms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_teaching_plans_grade_offerings_GradeOfferingId",
                        column: x => x.GradeOfferingId,
                        principalSchema: "school",
                        principalTable: "grade_offerings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_teaching_plans_grade_subject_offerings_GradeSubjectOfferingId",
                        column: x => x.GradeSubjectOfferingId,
                        principalSchema: "school",
                        principalTable: "grade_subject_offerings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_teaching_plans_program_academic_years_ProgramAcademicYearId",
                        column: x => x.ProgramAcademicYearId,
                        principalSchema: "school",
                        principalTable: "program_academic_years",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_teaching_plans_teaching_plans_ParentPlanId",
                        column: x => x.ParentPlanId,
                        principalSchema: "school",
                        principalTable: "teaching_plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "document_audits",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_audits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_document_audits_documents_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "school",
                        principalTable: "documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "document_versions",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Sha256 = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    StorageKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ValidationStatus = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_versions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_document_versions_documents_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "school",
                        principalTable: "documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "teaching_plan_audits",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeachingPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teaching_plan_audits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_teaching_plan_audits_teaching_plans_TeachingPlanId",
                        column: x => x.TeachingPlanId,
                        principalSchema: "school",
                        principalTable: "teaching_plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "teaching_plan_documents",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeachingPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teaching_plan_documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_teaching_plan_documents_documents_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "school",
                        principalTable: "documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_teaching_plan_documents_teaching_plans_TeachingPlanId",
                        column: x => x.TeachingPlanId,
                        principalSchema: "school",
                        principalTable: "teaching_plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "teaching_plan_items",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeachingPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    FromDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ToDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teaching_plan_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_teaching_plan_items_teaching_plans_TeachingPlanId",
                        column: x => x.TeachingPlanId,
                        principalSchema: "school",
                        principalTable: "teaching_plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "teaching_plan_targets",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeachingPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassSectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teaching_plan_targets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_teaching_plan_targets_class_sections_ClassSectionId",
                        column: x => x.ClassSectionId,
                        principalSchema: "school",
                        principalTable: "class_sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_teaching_plan_targets_teaching_plans_TeachingPlanId",
                        column: x => x.TeachingPlanId,
                        principalSchema: "school",
                        principalTable: "teaching_plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_document_audits_DocumentId",
                schema: "school",
                table: "document_audits",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_document_versions_DocumentId_VersionNumber",
                schema: "school",
                table: "document_versions",
                columns: new[] { "DocumentId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_teaching_plan_audits_TeachingPlanId",
                schema: "school",
                table: "teaching_plan_audits",
                column: "TeachingPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_teaching_plan_documents_DocumentId",
                schema: "school",
                table: "teaching_plan_documents",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_teaching_plan_documents_TeachingPlanId_DocumentId",
                schema: "school",
                table: "teaching_plan_documents",
                columns: new[] { "TeachingPlanId", "DocumentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_teaching_plan_items_TeachingPlanId",
                schema: "school",
                table: "teaching_plan_items",
                column: "TeachingPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_teaching_plan_targets_ClassSectionId",
                schema: "school",
                table: "teaching_plan_targets",
                column: "ClassSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_teaching_plan_targets_TeachingPlanId_ClassSectionId",
                schema: "school",
                table: "teaching_plan_targets",
                columns: new[] { "TeachingPlanId", "ClassSectionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_teaching_plans_AcademicTermId",
                schema: "school",
                table: "teaching_plans",
                column: "AcademicTermId");

            migrationBuilder.CreateIndex(
                name: "IX_teaching_plans_GradeOfferingId",
                schema: "school",
                table: "teaching_plans",
                column: "GradeOfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_teaching_plans_GradeSubjectOfferingId",
                schema: "school",
                table: "teaching_plans",
                column: "GradeSubjectOfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_teaching_plans_ParentPlanId",
                schema: "school",
                table: "teaching_plans",
                column: "ParentPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_teaching_plans_ProgramAcademicYearId",
                schema: "school",
                table: "teaching_plans",
                column: "ProgramAcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_teaching_plans_Type_GradeSubjectOfferingId_FromDate_ToDate",
                schema: "school",
                table: "teaching_plans",
                columns: new[] { "Type", "GradeSubjectOfferingId", "FromDate", "ToDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "document_audits",
                schema: "school");

            migrationBuilder.DropTable(
                name: "document_versions",
                schema: "school");

            migrationBuilder.DropTable(
                name: "teaching_plan_audits",
                schema: "school");

            migrationBuilder.DropTable(
                name: "teaching_plan_documents",
                schema: "school");

            migrationBuilder.DropTable(
                name: "teaching_plan_items",
                schema: "school");

            migrationBuilder.DropTable(
                name: "teaching_plan_targets",
                schema: "school");

            migrationBuilder.DropTable(
                name: "documents",
                schema: "school");

            migrationBuilder.DropTable(
                name: "teaching_plans",
                schema: "school");
        }
    }
}
