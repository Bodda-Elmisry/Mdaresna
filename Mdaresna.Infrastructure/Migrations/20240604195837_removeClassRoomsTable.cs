using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class removeClassRoomsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassRooms_ClassRoomLanguages_LanguageId",
                table: "ClassRooms");

            migrationBuilder.DropForeignKey(
                name: "FK_SchoolCourses_ClassRoomLanguages_LanguageId",
                table: "SchoolCourses");

            migrationBuilder.DropTable(
                name: "ClassRoomLanguages");

            migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifyDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Languages", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_ClassRooms_Languages_LanguageId",
                table: "ClassRooms",
                column: "LanguageId",
                principalTable: "Languages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SchoolCourses_Languages_LanguageId",
                table: "SchoolCourses",
                column: "LanguageId",
                principalTable: "Languages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassRooms_Languages_LanguageId",
                table: "ClassRooms");

            migrationBuilder.DropForeignKey(
                name: "FK_SchoolCourses_Languages_LanguageId",
                table: "SchoolCourses");

            migrationBuilder.DropTable(
                name: "Languages");

            migrationBuilder.CreateTable(
                name: "ClassRoomLanguages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifyDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassRoomLanguages", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ClassRoomLanguages",
                columns: new[] { "Id", "CreateDate", "Description", "LastModifyDate", "Name" },
                values: new object[,]
                {
                    { new Guid("42f73d52-5e31-485a-8f5b-6f53670447ca"), new DateTime(2024, 5, 29, 21, 47, 16, 119, DateTimeKind.Local).AddTicks(6445), "For english class rooms", new DateTime(2024, 5, 29, 21, 47, 16, 119, DateTimeKind.Local).AddTicks(6447), "English" },
                    { new Guid("7ff35951-0ad2-46c7-83a5-f4487365ec1c"), new DateTime(2024, 5, 29, 21, 47, 16, 119, DateTimeKind.Local).AddTicks(6390), "For arabic class rooms", new DateTime(2024, 5, 29, 21, 47, 16, 119, DateTimeKind.Local).AddTicks(6441), "Arabic" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_ClassRooms_ClassRoomLanguages_LanguageId",
                table: "ClassRooms",
                column: "LanguageId",
                principalTable: "ClassRoomLanguages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SchoolCourses_ClassRoomLanguages_LanguageId",
                table: "SchoolCourses",
                column: "LanguageId",
                principalTable: "ClassRoomLanguages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
