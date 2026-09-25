using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mdaresna.Schools.Infrastructure.Persistence.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentCoreAdmissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "guardians",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlatformAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    NationalId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guardians", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "students",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    GlobalStudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    FullNameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FullNameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    Gender = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    NationalId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    BirthCertificateNumber = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_students", x => x.Id);
                    table.ForeignKey(
                        name: "FK_students_persons_PersonId",
                        column: x => x.PersonId,
                        principalSchema: "school",
                        principalTable: "persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "admission_applications",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationNumber = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    GlobalStudentId = table.Column<Guid>(type: "uuid", nullable: true),
                    StudentCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    FullNameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FullNameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    Gender = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    NationalId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    BirthCertificateNumber = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    ProgramAcademicYearId = table.Column<Guid>(type: "uuid", nullable: false),
                    GradeLevelId = table.Column<Guid>(type: "uuid", nullable: false),
                    Source = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SubmittedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AcceptedStudentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admission_applications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_admission_applications_grade_levels_GradeLevelId",
                        column: x => x.GradeLevelId,
                        principalSchema: "school",
                        principalTable: "grade_levels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_admission_applications_program_academic_years_ProgramAcadem~",
                        column: x => x.ProgramAcademicYearId,
                        principalSchema: "school",
                        principalTable: "program_academic_years",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_admission_applications_students_AcceptedStudentId",
                        column: x => x.AcceptedStudentId,
                        principalSchema: "school",
                        principalTable: "students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "student_enrollments",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    GradeOfferingId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassSectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnrollmentDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_enrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_student_enrollments_class_sections_ClassSectionId",
                        column: x => x.ClassSectionId,
                        principalSchema: "school",
                        principalTable: "class_sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_student_enrollments_grade_offerings_GradeOfferingId",
                        column: x => x.GradeOfferingId,
                        principalSchema: "school",
                        principalTable: "grade_offerings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_student_enrollments_students_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "school",
                        principalTable: "students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "student_guardians",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    GuardianId = table.Column<Guid>(type: "uuid", nullable: false),
                    Relationship = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    CanPickup = table.Column<bool>(type: "boolean", nullable: false),
                    IsFinancialResponsible = table.Column<bool>(type: "boolean", nullable: false),
                    IsEmergencyContact = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_guardians", x => x.Id);
                    table.ForeignKey(
                        name: "FK_student_guardians_guardians_GuardianId",
                        column: x => x.GuardianId,
                        principalSchema: "school",
                        principalTable: "guardians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_student_guardians_students_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "school",
                        principalTable: "students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "admission_application_guardians",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdmissionApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    GuardianId = table.Column<Guid>(type: "uuid", nullable: false),
                    Relationship = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    CanPickup = table.Column<bool>(type: "boolean", nullable: false),
                    IsFinancialResponsible = table.Column<bool>(type: "boolean", nullable: false),
                    IsEmergencyContact = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admission_application_guardians", x => x.Id);
                    table.ForeignKey(
                        name: "FK_admission_application_guardians_admission_applications_Admi~",
                        column: x => x.AdmissionApplicationId,
                        principalSchema: "school",
                        principalTable: "admission_applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_admission_application_guardians_guardians_GuardianId",
                        column: x => x.GuardianId,
                        principalSchema: "school",
                        principalTable: "guardians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "school",
                table: "local_permissions",
                columns: new[] { "Id", "Code", "CreatedAtUtc", "DisplayNameAr", "DisplayNameEn", "IsActive", "Module", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd1c"), "school.students.view", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "عرض الطلاب", "View students", true, "students", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd1d"), "school.students.manage", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "إدارة الطلاب", "Manage students", true, "students", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd1e"), "school.admissions.view", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "عرض طلبات التقديم", "View admission applications", true, "admissions", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd1f"), "school.admissions.manage", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "إدارة طلبات التقديم", "Manage admission applications", true, "admissions", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.InsertData(
                schema: "school",
                table: "local_role_permissions",
                columns: new[] { "PermissionId", "RoleId", "GrantedAtUtc", "GrantedByUserId" },
                values: new object[,]
                {
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd1c"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd1d"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd1e"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd1f"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_admission_application_guardians_AdmissionApplicationId",
                schema: "school",
                table: "admission_application_guardians",
                column: "AdmissionApplicationId",
                unique: true,
                filter: "\"IsPrimary\" = TRUE");

            migrationBuilder.CreateIndex(
                name: "IX_admission_application_guardians_AdmissionApplicationId_Guar~",
                schema: "school",
                table: "admission_application_guardians",
                columns: new[] { "AdmissionApplicationId", "GuardianId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_admission_application_guardians_GuardianId",
                schema: "school",
                table: "admission_application_guardians",
                column: "GuardianId");

            migrationBuilder.CreateIndex(
                name: "IX_admission_applications_AcceptedStudentId",
                schema: "school",
                table: "admission_applications",
                column: "AcceptedStudentId");

            migrationBuilder.CreateIndex(
                name: "IX_admission_applications_ApplicationNumber",
                schema: "school",
                table: "admission_applications",
                column: "ApplicationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_admission_applications_GradeLevelId",
                schema: "school",
                table: "admission_applications",
                column: "GradeLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_admission_applications_NormalizedName_DateOfBirth",
                schema: "school",
                table: "admission_applications",
                columns: new[] { "NormalizedName", "DateOfBirth" });

            migrationBuilder.CreateIndex(
                name: "IX_admission_applications_ProgramAcademicYearId_GradeLevelId",
                schema: "school",
                table: "admission_applications",
                columns: new[] { "ProgramAcademicYearId", "GradeLevelId" });

            migrationBuilder.CreateIndex(
                name: "IX_admission_applications_Status_SubmittedAtUtc",
                schema: "school",
                table: "admission_applications",
                columns: new[] { "Status", "SubmittedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_guardians_Phone",
                schema: "school",
                table: "guardians",
                column: "Phone");

            migrationBuilder.CreateIndex(
                name: "IX_student_enrollments_ClassSectionId_Status",
                schema: "school",
                table: "student_enrollments",
                columns: new[] { "ClassSectionId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_student_enrollments_GradeOfferingId",
                schema: "school",
                table: "student_enrollments",
                column: "GradeOfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_student_enrollments_StudentId_GradeOfferingId",
                schema: "school",
                table: "student_enrollments",
                columns: new[] { "StudentId", "GradeOfferingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_student_guardians_GuardianId",
                schema: "school",
                table: "student_guardians",
                column: "GuardianId");

            migrationBuilder.CreateIndex(
                name: "IX_student_guardians_StudentId",
                schema: "school",
                table: "student_guardians",
                column: "StudentId",
                unique: true,
                filter: "\"IsActive\" = TRUE AND \"IsPrimary\" = TRUE");

            migrationBuilder.CreateIndex(
                name: "IX_student_guardians_StudentId_GuardianId",
                schema: "school",
                table: "student_guardians",
                columns: new[] { "StudentId", "GuardianId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_students_GlobalStudentId",
                schema: "school",
                table: "students",
                column: "GlobalStudentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_students_NormalizedName_DateOfBirth",
                schema: "school",
                table: "students",
                columns: new[] { "NormalizedName", "DateOfBirth" });

            migrationBuilder.CreateIndex(
                name: "IX_students_PersonId",
                schema: "school",
                table: "students",
                column: "PersonId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_students_StudentCode",
                schema: "school",
                table: "students",
                column: "StudentCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admission_application_guardians",
                schema: "school");

            migrationBuilder.DropTable(
                name: "student_enrollments",
                schema: "school");

            migrationBuilder.DropTable(
                name: "student_guardians",
                schema: "school");

            migrationBuilder.DropTable(
                name: "admission_applications",
                schema: "school");

            migrationBuilder.DropTable(
                name: "guardians",
                schema: "school");

            migrationBuilder.DropTable(
                name: "students",
                schema: "school");

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd1c"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd1d"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd1e"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd1f"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd1c"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd1d"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd1e"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd1f"));
        }
    }
}
