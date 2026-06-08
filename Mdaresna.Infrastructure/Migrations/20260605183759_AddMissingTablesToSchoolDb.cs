using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingTablesToSchoolDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReportQueues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GradeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClassroomId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MonthId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    WeekName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReportType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValue: new DateTime(2026, 6, 5, 18, 37, 58, 296, DateTimeKind.Utc).AddTicks(7448)),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewdById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Errors = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AffectedRows = table.Column<int>(type: "int", nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportQueues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportQueues_ClassRooms_ClassroomId",
                        column: x => x.ClassroomId,
                        principalTable: "ClassRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReportQueues_SchoolGrades_GradeId",
                        column: x => x.GradeId,
                        principalTable: "SchoolGrades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReportQueues_SchoolYearMonths_MonthId",
                        column: x => x.MonthId,
                        principalTable: "SchoolYearMonths",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReportQueues_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReportQueues_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReportQueues_Users_ReviewdById",
                        column: x => x.ReviewdById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportQueues_ClassroomId",
                table: "ReportQueues",
                column: "ClassroomId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportQueues_CreatedById",
                table: "ReportQueues",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ReportQueues_GradeId",
                table: "ReportQueues",
                column: "GradeId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportQueues_MonthId",
                table: "ReportQueues",
                column: "MonthId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportQueues_ReviewdById",
                table: "ReportQueues",
                column: "ReviewdById");

            migrationBuilder.CreateIndex(
                name: "IX_ReportQueues_SchoolId",
                table: "ReportQueues",
                column: "SchoolId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportQueues");
        }
    }
}
