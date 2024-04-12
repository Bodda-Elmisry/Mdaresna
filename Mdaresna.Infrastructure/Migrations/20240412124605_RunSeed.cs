using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RunSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "AppPermission", "Description", "Name", "SchoolPermission" },
                values: new object[] { new Guid("0225dbd5-9675-438c-87f2-63fb6921841c"), true, "Assign School Manager Permission To User", "AssignScoolManagerToUser", false });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("0225dbd5-9675-438c-87f2-63fb6921841c"));
        }
    }
}
