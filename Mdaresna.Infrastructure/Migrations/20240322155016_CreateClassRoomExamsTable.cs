using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateClassRoomExamsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClassRoomExams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WeekDay = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClassRoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupervisorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MonthId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassRoomExams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassRoomExams_ClassRooms_ClassRoomId",
                        column: x => x.ClassRoomId,
                        principalTable: "ClassRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassRoomExams_SchoolCourses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "SchoolCourses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassRoomExams_SchoolYearMonths_MonthId",
                        column: x => x.MonthId,
                        principalTable: "SchoolYearMonths",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassRoomExams_Users_SupervisorId",
                        column: x => x.SupervisorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassRoomExams_ClassRoomId",
                table: "ClassRoomExams",
                column: "ClassRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassRoomExams_CourseId",
                table: "ClassRoomExams",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassRoomExams_MonthId",
                table: "ClassRoomExams",
                column: "MonthId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassRoomExams_SupervisorId",
                table: "ClassRoomExams",
                column: "SupervisorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassRoomExams");
        }
    }
}
