using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreatestudentExamRatesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "studentExamRates",
                columns: table => new
                {
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RateHeaderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Result = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_studentExamRates", x => new { x.StudentId, x.ExamId, x.RateHeaderId });
                    table.ForeignKey(
                        name: "FK_studentExamRates_ClassRoomExams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "ClassRoomExams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_studentExamRates_SchoolExamRateHeaders_RateHeaderId",
                        column: x => x.RateHeaderId,
                        principalTable: "SchoolExamRateHeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_studentExamRates_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_studentExamRates_ExamId",
                table: "studentExamRates",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_studentExamRates_RateHeaderId",
                table: "studentExamRates",
                column: "RateHeaderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "studentExamRates");
        }
    }
}
