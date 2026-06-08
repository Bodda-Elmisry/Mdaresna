using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReportStatsToSchoolYearMonths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReportStatus",
                table: "SchoolYearMonths",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ReportQueues",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 6, 5, 19, 31, 37, 303, DateTimeKind.Utc).AddTicks(3846));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReportStatus",
                table: "SchoolYearMonths");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ReportQueues",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 6, 5, 19, 31, 37, 303, DateTimeKind.Utc).AddTicks(3846),
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }
    }
}
