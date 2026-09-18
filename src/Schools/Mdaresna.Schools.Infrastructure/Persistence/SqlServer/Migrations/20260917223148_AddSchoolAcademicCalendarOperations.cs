using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mdaresna.Schools.Infrastructure.Persistence.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolAcademicCalendarOperations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "academic_year_definitions",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
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
                    table.PrimaryKey("PK_academic_year_definitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "education_programs",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ProgramType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
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
                    table.PrimaryKey("PK_education_programs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "program_academic_years",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EducationProgramId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
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
                    table.PrimaryKey("PK_program_academic_years", x => x.Id);
                    table.ForeignKey(
                        name: "FK_program_academic_years_academic_year_definitions_AcademicYearDefinitionId",
                        column: x => x.AcademicYearDefinitionId,
                        principalSchema: "school",
                        principalTable: "academic_year_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_program_academic_years_education_programs_EducationProgramId",
                        column: x => x.EducationProgramId,
                        principalSchema: "school",
                        principalTable: "education_programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "school_day_schedules",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EducationProgramId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_school_day_schedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_school_day_schedules_education_programs_EducationProgramId",
                        column: x => x.EducationProgramId,
                        principalSchema: "school",
                        principalTable: "education_programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_school_day_schedules_school_branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "school",
                        principalTable: "school_branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "academic_terms",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProgramAcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_academic_terms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_academic_terms_program_academic_years_ProgramAcademicYearId",
                        column: x => x.ProgramAcademicYearId,
                        principalSchema: "school",
                        principalTable: "program_academic_years",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "school_calendar_events",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EducationProgramId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProgramAcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsSchoolClosed = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_school_calendar_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_school_calendar_events_education_programs_EducationProgramId",
                        column: x => x.EducationProgramId,
                        principalSchema: "school",
                        principalTable: "education_programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_school_calendar_events_program_academic_years_ProgramAcademicYearId",
                        column: x => x.ProgramAcademicYearId,
                        principalSchema: "school",
                        principalTable: "program_academic_years",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_school_calendar_events_school_branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "school",
                        principalTable: "school_branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "academic_periods",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicTermId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_academic_periods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_academic_periods_academic_terms_AcademicTermId",
                        column: x => x.AcademicTermId,
                        principalSchema: "school",
                        principalTable: "academic_terms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "school",
                table: "local_permissions",
                columns: new[] { "Id", "Code", "CreatedAtUtc", "DisplayNameAr", "DisplayNameEn", "IsActive", "Module", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd0c"), "school.academics.view", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "عرض الهيكلة الأكاديمية", "View academic structure", true, "academics", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd0d"), "school.academics.manage", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "إدارة الهيكلة الأكاديمية", "Manage academic structure", true, "academics", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd0e"), "school.academics.delete", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "حذف عناصر الهيكلة الأكاديمية", "Delete academic structure", true, "academics", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd0f"), "school.academics.restore", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "استعادة عناصر الهيكلة الأكاديمية", "Restore academic structure", true, "academics", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd10"), "school.operations.view", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "عرض إعدادات التشغيل", "View operations", true, "operations", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd11"), "school.operations.manage", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "إدارة إعدادات التشغيل", "Manage operations", true, "operations", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd12"), "school.operations.delete", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "حذف إعدادات التشغيل", "Delete operations", true, "operations", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd13"), "school.operations.restore", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "استعادة إعدادات التشغيل", "Restore operations", true, "operations", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd14"), "school.calendar.view", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "عرض التقويم المدرسي", "View school calendar", true, "calendar", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd15"), "school.calendar.manage", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "إدارة التقويم المدرسي", "Manage school calendar", true, "calendar", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd16"), "school.calendar.delete", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "حذف أحداث التقويم", "Delete calendar events", true, "calendar", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd17"), "school.calendar.restore", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "استعادة أحداث التقويم", "Restore calendar events", true, "calendar", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.InsertData(
                schema: "school",
                table: "local_role_permissions",
                columns: new[] { "PermissionId", "RoleId", "GrantedAtUtc", "GrantedByUserId" },
                values: new object[,]
                {
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd0c"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd0d"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd0e"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd0f"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd10"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd11"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd12"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd13"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd14"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd15"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd16"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd17"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_academic_periods_AcademicTermId_Code",
                schema: "school",
                table: "academic_periods",
                columns: new[] { "AcademicTermId", "Code" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_academic_terms_ProgramAcademicYearId_Code",
                schema: "school",
                table: "academic_terms",
                columns: new[] { "ProgramAcademicYearId", "Code" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_academic_year_definitions_Code",
                schema: "school",
                table: "academic_year_definitions",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_education_programs_Code",
                schema: "school",
                table: "education_programs",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_program_academic_years_AcademicYearDefinitionId",
                schema: "school",
                table: "program_academic_years",
                column: "AcademicYearDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_program_academic_years_EducationProgramId_AcademicYearDefinitionId",
                schema: "school",
                table: "program_academic_years",
                columns: new[] { "EducationProgramId", "AcademicYearDefinitionId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_school_calendar_events_BranchId",
                schema: "school",
                table: "school_calendar_events",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_school_calendar_events_Code",
                schema: "school",
                table: "school_calendar_events",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_school_calendar_events_EducationProgramId",
                schema: "school",
                table: "school_calendar_events",
                column: "EducationProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_school_calendar_events_ProgramAcademicYearId",
                schema: "school",
                table: "school_calendar_events",
                column: "ProgramAcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_school_day_schedules_BranchId",
                schema: "school",
                table: "school_day_schedules",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_school_day_schedules_EducationProgramId_BranchId_DayOfWeek",
                schema: "school",
                table: "school_day_schedules",
                columns: new[] { "EducationProgramId", "BranchId", "DayOfWeek" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "academic_periods",
                schema: "school");

            migrationBuilder.DropTable(
                name: "school_calendar_events",
                schema: "school");

            migrationBuilder.DropTable(
                name: "school_day_schedules",
                schema: "school");

            migrationBuilder.DropTable(
                name: "academic_terms",
                schema: "school");

            migrationBuilder.DropTable(
                name: "program_academic_years",
                schema: "school");

            migrationBuilder.DropTable(
                name: "academic_year_definitions",
                schema: "school");

            migrationBuilder.DropTable(
                name: "education_programs",
                schema: "school");

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd0c"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd0d"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd0e"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd0f"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd10"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd11"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd12"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd13"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd14"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd15"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd16"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_role_permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd17"), new Guid("62f4655a-20af-4acc-bb74-c42733e4f713") });

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd0c"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd0d"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd0e"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd0f"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd10"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd11"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd12"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd13"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd14"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd15"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd16"));

            migrationBuilder.DeleteData(
                schema: "school",
                table: "local_permissions",
                keyColumn: "Id",
                keyValue: new Guid("73d1e183-1cad-4fe2-99c5-b39dc7fffd17"));
        }
    }
}
