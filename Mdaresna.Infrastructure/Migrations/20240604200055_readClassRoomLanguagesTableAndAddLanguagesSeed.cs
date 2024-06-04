using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class readClassRoomLanguagesTableAndAddLanguagesSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClassRoomLanguages",
                columns: table => new
                {
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LanguageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifyDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassRoomLanguages", x => new { x.SchoolId, x.LanguageId });
                    table.ForeignKey(
                        name: "FK_ClassRoomLanguages_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassRoomLanguages_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Languages",
                columns: new[] { "Id", "CreateDate", "Description", "LastModifyDate", "Name" },
                values: new object[,]
                {
                    { new Guid("42f73d52-5e31-485a-8f5b-6f53670447ca"), new DateTime(2024, 6, 4, 23, 0, 54, 602, DateTimeKind.Local).AddTicks(9882), "For english class rooms", new DateTime(2024, 6, 4, 23, 0, 54, 602, DateTimeKind.Local).AddTicks(9884), "English" },
                    { new Guid("7ff35951-0ad2-46c7-83a5-f4487365ec1c"), new DateTime(2024, 6, 4, 23, 0, 54, 602, DateTimeKind.Local).AddTicks(9807), "For arabic class rooms", new DateTime(2024, 6, 4, 23, 0, 54, 602, DateTimeKind.Local).AddTicks(9877), "Arabic" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassRoomLanguages_LanguageId",
                table: "ClassRoomLanguages",
                column: "LanguageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassRoomLanguages");

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("42f73d52-5e31-485a-8f5b-6f53670447ca"));

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("7ff35951-0ad2-46c7-83a5-f4487365ec1c"));
        }
    }
}
