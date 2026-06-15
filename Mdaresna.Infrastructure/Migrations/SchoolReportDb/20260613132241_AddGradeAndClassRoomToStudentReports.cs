using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations.SchoolReportDb
{
    /// <inheritdoc />
    public partial class AddGradeAndClassRoomToStudentReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClassRoomId",
                table: "StudentReports",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GradeId",
                table: "StudentReports",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentReports_ClassRoomId",
                table: "StudentReports",
                column: "ClassRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentReports_GradeId",
                table: "StudentReports",
                column: "GradeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudentReports_ClassRoomId",
                table: "StudentReports");

            migrationBuilder.DropIndex(
                name: "IX_StudentReports_GradeId",
                table: "StudentReports");

            migrationBuilder.DropColumn(
                name: "ClassRoomId",
                table: "StudentReports");

            migrationBuilder.DropColumn(
                name: "GradeId",
                table: "StudentReports");
        }
    }
}
