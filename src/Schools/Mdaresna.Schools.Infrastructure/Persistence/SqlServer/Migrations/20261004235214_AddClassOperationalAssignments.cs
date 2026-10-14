using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Schools.Infrastructure.Persistence.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddClassOperationalAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActualTeacherNameSnapshot",
                schema: "school",
                table: "student_attendance_registers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ActualTeacherUserIdSnapshot",
                schema: "school",
                table: "student_attendance_registers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastEditedByNameSnapshot",
                schema: "school",
                table: "student_attendance_registers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastEditedByUserId",
                schema: "school",
                table: "student_attendance_registers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryTeacherNameSnapshot",
                schema: "school",
                table: "student_attendance_registers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PrimaryTeacherUserIdSnapshot",
                schema: "school",
                table: "student_attendance_registers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecordedByKindSnapshot",
                schema: "school",
                table: "student_attendance_registers",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Revision",
                schema: "school",
                table: "student_attendance_registers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SlotNumberSnapshot",
                schema: "school",
                table: "student_attendance_registers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubjectNameSnapshot",
                schema: "school",
                table: "student_attendance_registers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "class_operational_assignments",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuildingFloorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Responsibility = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StartsOn = table.Column<DateOnly>(type: "date", nullable: false),
                    EndsOn = table.Column<DateOnly>(type: "date", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Revision = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_class_operational_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_class_operational_assignments_building_floors_BuildingFloorId",
                        column: x => x.BuildingFloorId,
                        principalSchema: "school",
                        principalTable: "building_floors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_class_operational_assignments_local_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "school",
                        principalTable: "local_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "class_operational_assignment_audits",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    SnapshotJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_class_operational_assignment_audits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_class_operational_assignment_audits_class_operational_assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalSchema: "school",
                        principalTable: "class_operational_assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "class_operational_grants",
                schema: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassSectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    GradeSubjectOfferingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_class_operational_grants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_class_operational_grants_class_operational_assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalSchema: "school",
                        principalTable: "class_operational_assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_class_operational_grants_class_sections_ClassSectionId",
                        column: x => x.ClassSectionId,
                        principalSchema: "school",
                        principalTable: "class_sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_class_operational_grants_grade_subject_offerings_GradeSubjectOfferingId",
                        column: x => x.GradeSubjectOfferingId,
                        principalSchema: "school",
                        principalTable: "grade_subject_offerings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_class_operational_assignment_audits_AssignmentId",
                schema: "school",
                table: "class_operational_assignment_audits",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_class_operational_assignments_BuildingFloorId",
                schema: "school",
                table: "class_operational_assignments",
                column: "BuildingFloorId");

            migrationBuilder.CreateIndex(
                name: "IX_class_operational_assignments_UserId_IsActive_StartsOn_EndsOn",
                schema: "school",
                table: "class_operational_assignments",
                columns: new[] { "UserId", "IsActive", "StartsOn", "EndsOn" });

            migrationBuilder.CreateIndex(
                name: "IX_class_operational_grants_AssignmentId",
                schema: "school",
                table: "class_operational_grants",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_class_operational_grants_ClassSectionId_Action",
                schema: "school",
                table: "class_operational_grants",
                columns: new[] { "ClassSectionId", "Action" });

            migrationBuilder.CreateIndex(
                name: "IX_class_operational_grants_GradeSubjectOfferingId",
                schema: "school",
                table: "class_operational_grants",
                column: "GradeSubjectOfferingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "class_operational_assignment_audits",
                schema: "school");

            migrationBuilder.DropTable(
                name: "class_operational_grants",
                schema: "school");

            migrationBuilder.DropTable(
                name: "class_operational_assignments",
                schema: "school");

            migrationBuilder.DropColumn(
                name: "ActualTeacherNameSnapshot",
                schema: "school",
                table: "student_attendance_registers");

            migrationBuilder.DropColumn(
                name: "ActualTeacherUserIdSnapshot",
                schema: "school",
                table: "student_attendance_registers");

            migrationBuilder.DropColumn(
                name: "LastEditedByNameSnapshot",
                schema: "school",
                table: "student_attendance_registers");

            migrationBuilder.DropColumn(
                name: "LastEditedByUserId",
                schema: "school",
                table: "student_attendance_registers");

            migrationBuilder.DropColumn(
                name: "PrimaryTeacherNameSnapshot",
                schema: "school",
                table: "student_attendance_registers");

            migrationBuilder.DropColumn(
                name: "PrimaryTeacherUserIdSnapshot",
                schema: "school",
                table: "student_attendance_registers");

            migrationBuilder.DropColumn(
                name: "RecordedByKindSnapshot",
                schema: "school",
                table: "student_attendance_registers");

            migrationBuilder.DropColumn(
                name: "Revision",
                schema: "school",
                table: "student_attendance_registers");

            migrationBuilder.DropColumn(
                name: "SlotNumberSnapshot",
                schema: "school",
                table: "student_attendance_registers");

            migrationBuilder.DropColumn(
                name: "SubjectNameSnapshot",
                schema: "school",
                table: "student_attendance_registers");
        }
    }
}
