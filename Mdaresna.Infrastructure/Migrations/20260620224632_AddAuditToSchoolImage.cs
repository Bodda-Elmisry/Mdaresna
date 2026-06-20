using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditToSchoolImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "SchoolImages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "SchoolImages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "SchoolImages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("42f73d52-5e31-485a-8f5b-6f53670447ca"),
                columns: new[] { "CreateDate", "LastModifyDate" },
                values: new object[] { new DateTime(2024, 6, 4, 20, 0, 54, 602, DateTimeKind.Utc), new DateTime(2024, 6, 4, 20, 0, 54, 602, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("7ff35951-0ad2-46c7-83a5-f4487365ec1c"),
                columns: new[] { "CreateDate", "LastModifyDate" },
                values: new object[] { new DateTime(2024, 6, 4, 20, 0, 54, 602, DateTimeKind.Utc), new DateTime(2024, 6, 4, 20, 0, 54, 602, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Schools",
                keyColumn: "Id",
                keyValue: new Guid("3338e17d-280f-467f-938a-5629415b6e52"),
                columns: new[] { "CreateDate", "LastModifyDate" },
                values: new object[] { new DateTime(2024, 9, 27, 18, 6, 0, 252, DateTimeKind.Utc).AddTicks(9751), new DateTime(2024, 9, 27, 18, 6, 0, 252, DateTimeKind.Utc).AddTicks(9751) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("d4c7be15-c9b6-4d83-8516-aff52c94f963"),
                columns: new[] { "CreateDate", "LastModifyDate" },
                values: new object[] { new DateTime(2024, 9, 27, 18, 6, 0, 252, DateTimeKind.Utc).AddTicks(9751), new DateTime(2024, 9, 27, 18, 6, 0, 252, DateTimeKind.Utc).AddTicks(9751) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("de36f342-fe3c-46c3-bdfc-bb3fcf2ec7e4"),
                columns: new[] { "CreateDate", "LastModifyDate" },
                values: new object[] { new DateTime(2024, 9, 27, 18, 6, 0, 252, DateTimeKind.Utc).AddTicks(9751), new DateTime(2024, 9, 27, 18, 6, 0, 252, DateTimeKind.Utc).AddTicks(9751) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "SchoolImages");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "SchoolImages");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "SchoolImages");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("42f73d52-5e31-485a-8f5b-6f53670447ca"),
                columns: new[] { "CreateDate", "LastModifyDate" },
                values: new object[] { new DateTime(2024, 6, 4, 23, 0, 54, 602, DateTimeKind.Local), new DateTime(2024, 6, 4, 23, 0, 54, 602, DateTimeKind.Local) });

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("7ff35951-0ad2-46c7-83a5-f4487365ec1c"),
                columns: new[] { "CreateDate", "LastModifyDate" },
                values: new object[] { new DateTime(2024, 6, 4, 23, 0, 54, 602, DateTimeKind.Local), new DateTime(2024, 6, 4, 23, 0, 54, 602, DateTimeKind.Local) });

            migrationBuilder.UpdateData(
                table: "Schools",
                keyColumn: "Id",
                keyValue: new Guid("3338e17d-280f-467f-938a-5629415b6e52"),
                columns: new[] { "CreateDate", "LastModifyDate" },
                values: new object[] { new DateTime(2024, 9, 27, 21, 6, 0, 252, DateTimeKind.Unspecified).AddTicks(9751), new DateTime(2024, 9, 27, 21, 6, 0, 252, DateTimeKind.Unspecified).AddTicks(9751) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("d4c7be15-c9b6-4d83-8516-aff52c94f963"),
                columns: new[] { "CreateDate", "LastModifyDate" },
                values: new object[] { new DateTime(2024, 9, 27, 21, 6, 0, 252, DateTimeKind.Unspecified).AddTicks(9751), new DateTime(2024, 9, 27, 21, 6, 0, 252, DateTimeKind.Unspecified).AddTicks(9751) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("de36f342-fe3c-46c3-bdfc-bb3fcf2ec7e4"),
                columns: new[] { "CreateDate", "LastModifyDate" },
                values: new object[] { new DateTime(2024, 9, 27, 21, 6, 0, 252, DateTimeKind.Unspecified).AddTicks(9751), new DateTime(2024, 9, 27, 21, 6, 0, 252, DateTimeKind.Unspecified).AddTicks(9751) });
        }
    }
}
