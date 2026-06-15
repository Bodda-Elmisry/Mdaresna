using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations.SchoolReportDb
{
    /// <inheritdoc />
    public partial class AddReportQueueIdToStudentReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReportQueueId",
                table: "StudentReports",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentReports_ReportQueueId",
                table: "StudentReports",
                column: "ReportQueueId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudentReports_ReportQueueId",
                table: "StudentReports");

            migrationBuilder.DropColumn(
                name: "ReportQueueId",
                table: "StudentReports");
        }
    }
}
