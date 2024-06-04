using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ClassRoomLanguages",
                columns: new[] { "Id", "CreateDate", "Description", "LastModifyDate", "Name" },
                values: new object[,]
                {
                    { new Guid("42f73d52-5e31-485a-8f5b-6f53670447ca"), new DateTime(2024, 5, 29, 21, 47, 16, 119, DateTimeKind.Local).AddTicks(6445), "For english class rooms", new DateTime(2024, 5, 29, 21, 47, 16, 119, DateTimeKind.Local).AddTicks(6447), "English" },
                    { new Guid("7ff35951-0ad2-46c7-83a5-f4487365ec1c"), new DateTime(2024, 5, 29, 21, 47, 16, 119, DateTimeKind.Local).AddTicks(6390), "For arabic class rooms", new DateTime(2024, 5, 29, 21, 47, 16, 119, DateTimeKind.Local).AddTicks(6441), "Arabic" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ClassRoomLanguages",
                keyColumn: "Id",
                keyValue: new Guid("42f73d52-5e31-485a-8f5b-6f53670447ca"));

            migrationBuilder.DeleteData(
                table: "ClassRoomLanguages",
                keyColumn: "Id",
                keyValue: new Guid("7ff35951-0ad2-46c7-83a5-f4487365ec1c"));
        }
    }
}
