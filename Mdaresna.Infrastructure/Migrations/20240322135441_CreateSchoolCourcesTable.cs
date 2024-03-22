using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateSchoolCourcesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SchoolCourses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LanguageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolCourses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchoolCourses_ClassRoomLanguages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "ClassRoomLanguages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchoolCourses_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SchoolCourses_LanguageId",
                table: "SchoolCourses",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolCourses_SchoolId",
                table: "SchoolCourses",
                column: "SchoolId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SchoolCourses");
        }
    }
}
