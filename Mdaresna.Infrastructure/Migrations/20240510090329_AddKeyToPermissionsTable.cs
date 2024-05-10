using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddKeyToPermissionsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Key",
                table: "Permissions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("0225dbd5-9675-438c-87f2-63fb6921841c"),
                columns: new[] { "Key", "Name" },
                values: new object[] { "AssignScoolManagerToUser", "Assign Scool Manager To User" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("219007ea-620e-4d96-8292-2d015ef68db1"),
                columns: new[] { "Key", "Name" },
                values: new object[] { "ViewChildernsList", "View Childerns" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("9301fc37-ae75-4ef6-b6fa-a656452e5a2e"),
                columns: new[] { "Key", "Name" },
                values: new object[] { "CreateSchool", "Add School" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Key",
                table: "Permissions");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("0225dbd5-9675-438c-87f2-63fb6921841c"),
                column: "Name",
                value: "AssignScoolManagerToUser");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("219007ea-620e-4d96-8292-2d015ef68db1"),
                column: "Name",
                value: "ViewChildernsList");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("9301fc37-ae75-4ef6-b6fa-a656452e5a2e"),
                column: "Name",
                value: "CreateSchool");
        }
    }
}
