using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mdaresna.Schools.Infrastructure.Persistence.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolCurriculumBooks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CurriculumPlanId",
                schema: "school",
                table: "program_academic_years",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "book_roles",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_book_roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "books",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Publisher = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("PK_books", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "curriculum_plans",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EducationProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    VersionLabel = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
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
                    table.PrimaryKey("PK_curriculum_plans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_curriculum_plans_education_programs_EducationProgramId",
                        column: x => x.EducationProgramId,
                        principalSchema: "school",
                        principalTable: "education_programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "subjects",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
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
                    table.PrimaryKey("PK_subjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "book_versions",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookId = table.Column<Guid>(type: "uuid", nullable: false),
                    EditionCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    VersionLabel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PublicationYear = table.Column<int>(type: "integer", nullable: false),
                    Language = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Isbn = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
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
                    table.PrimaryKey("PK_book_versions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_book_versions_books_BookId",
                        column: x => x.BookId,
                        principalSchema: "school",
                        principalTable: "books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "curriculum_grade_subjects",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CurriculumPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    GradeLevelId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    TermNumber = table.Column<int>(type: "integer", nullable: false),
                    WeeklyPeriods = table.Column<int>(type: "integer", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    InstructionLanguage = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_curriculum_grade_subjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_curriculum_grade_subjects_curriculum_plans_CurriculumPlanId",
                        column: x => x.CurriculumPlanId,
                        principalSchema: "school",
                        principalTable: "curriculum_plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_curriculum_grade_subjects_grade_levels_GradeLevelId",
                        column: x => x.GradeLevelId,
                        principalSchema: "school",
                        principalTable: "grade_levels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_curriculum_grade_subjects_subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalSchema: "school",
                        principalTable: "subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "curriculum_subject_books",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CurriculumGradeSubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    BookVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    BookRoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_curriculum_subject_books", x => x.Id);
                    table.ForeignKey(
                        name: "FK_curriculum_subject_books_book_roles_BookRoleId",
                        column: x => x.BookRoleId,
                        principalSchema: "school",
                        principalTable: "book_roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_curriculum_subject_books_book_versions_BookVersionId",
                        column: x => x.BookVersionId,
                        principalSchema: "school",
                        principalTable: "book_versions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_curriculum_subject_books_curriculum_grade_subjects_Curricul~",
                        column: x => x.CurriculumGradeSubjectId,
                        principalSchema: "school",
                        principalTable: "curriculum_grade_subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "grade_subject_offerings",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GradeOfferingId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurriculumGradeSubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
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
                    table.PrimaryKey("PK_grade_subject_offerings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_grade_subject_offerings_curriculum_grade_subjects_Curriculu~",
                        column: x => x.CurriculumGradeSubjectId,
                        principalSchema: "school",
                        principalTable: "curriculum_grade_subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_grade_subject_offerings_grade_offerings_GradeOfferingId",
                        column: x => x.GradeOfferingId,
                        principalSchema: "school",
                        principalTable: "grade_offerings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "school",
                table: "book_roles",
                columns: new[] { "Id", "Code", "CreatedAtUtc", "DeletedAtUtc", "DeletedByUserId", "IsActive", "IsDeleted", "IsSystem", "NameAr", "NameEn", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { new Guid("b1000000-0000-0000-0000-000000000001"), "PRIMARY", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, true, false, true, "الكتاب الأساسي", "Primary book", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("b1000000-0000-0000-0000-000000000002"), "WORKBOOK", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, true, false, true, "كتاب التدريبات", "Workbook", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("b1000000-0000-0000-0000-000000000003"), "TEACHER_GUIDE", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, true, false, true, "دليل المعلم", "Teacher guide", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("b1000000-0000-0000-0000-000000000004"), "SUPPLEMENTARY", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, true, false, true, "كتاب مساعد", "Supplementary book", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("b1000000-0000-0000-0000-000000000005"), "REFERENCE", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, true, false, true, "مرجع", "Reference", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("b1000000-0000-0000-0000-000000000006"), "ACTIVITY_BOOK", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, true, false, true, "كتاب الأنشطة", "Activity book", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_program_academic_years_CurriculumPlanId",
                schema: "school",
                table: "program_academic_years",
                column: "CurriculumPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_book_roles_Code",
                schema: "school",
                table: "book_roles",
                column: "Code",
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_book_versions_BookId_EditionCode",
                schema: "school",
                table: "book_versions",
                columns: new[] { "BookId", "EditionCode" },
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_books_Code",
                schema: "school",
                table: "books",
                column: "Code",
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_curriculum_grade_subjects_CurriculumPlanId_GradeLevelId_Sub~",
                schema: "school",
                table: "curriculum_grade_subjects",
                columns: new[] { "CurriculumPlanId", "GradeLevelId", "SubjectId", "TermNumber" },
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_curriculum_grade_subjects_GradeLevelId",
                schema: "school",
                table: "curriculum_grade_subjects",
                column: "GradeLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_curriculum_grade_subjects_SubjectId",
                schema: "school",
                table: "curriculum_grade_subjects",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_curriculum_plans_EducationProgramId_Code_VersionLabel",
                schema: "school",
                table: "curriculum_plans",
                columns: new[] { "EducationProgramId", "Code", "VersionLabel" },
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_curriculum_subject_books_BookRoleId",
                schema: "school",
                table: "curriculum_subject_books",
                column: "BookRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_curriculum_subject_books_BookVersionId",
                schema: "school",
                table: "curriculum_subject_books",
                column: "BookVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_curriculum_subject_books_CurriculumGradeSubjectId_BookVersi~",
                schema: "school",
                table: "curriculum_subject_books",
                columns: new[] { "CurriculumGradeSubjectId", "BookVersionId", "BookRoleId" },
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_grade_subject_offerings_CurriculumGradeSubjectId",
                schema: "school",
                table: "grade_subject_offerings",
                column: "CurriculumGradeSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_grade_subject_offerings_GradeOfferingId_CurriculumGradeSubj~",
                schema: "school",
                table: "grade_subject_offerings",
                columns: new[] { "GradeOfferingId", "CurriculumGradeSubjectId" },
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_subjects_Code",
                schema: "school",
                table: "subjects",
                column: "Code",
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.AddForeignKey(
                name: "FK_program_academic_years_curriculum_plans_CurriculumPlanId",
                schema: "school",
                table: "program_academic_years",
                column: "CurriculumPlanId",
                principalSchema: "school",
                principalTable: "curriculum_plans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_program_academic_years_curriculum_plans_CurriculumPlanId",
                schema: "school",
                table: "program_academic_years");

            migrationBuilder.DropTable(
                name: "curriculum_subject_books",
                schema: "school");

            migrationBuilder.DropTable(
                name: "grade_subject_offerings",
                schema: "school");

            migrationBuilder.DropTable(
                name: "book_roles",
                schema: "school");

            migrationBuilder.DropTable(
                name: "book_versions",
                schema: "school");

            migrationBuilder.DropTable(
                name: "curriculum_grade_subjects",
                schema: "school");

            migrationBuilder.DropTable(
                name: "books",
                schema: "school");

            migrationBuilder.DropTable(
                name: "curriculum_plans",
                schema: "school");

            migrationBuilder.DropTable(
                name: "subjects",
                schema: "school");

            migrationBuilder.DropIndex(
                name: "IX_program_academic_years_CurriculumPlanId",
                schema: "school",
                table: "program_academic_years");

            migrationBuilder.DropColumn(
                name: "CurriculumPlanId",
                schema: "school",
                table: "program_academic_years");
        }
    }
}
