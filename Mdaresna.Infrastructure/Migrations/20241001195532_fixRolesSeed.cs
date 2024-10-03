using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class fixRolesSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("10620c5f-37fe-4d18-996f-915ece8893f1"),
                columns: new[] { "AdminRole", "SchoolRole" },
                values: new object[] { false, true });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"),
                columns: new[] { "AdminRole", "SchoolRole" },
                values: new object[] { false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("10620c5f-37fe-4d18-996f-915ece8893f1"),
                columns: new[] { "AdminRole", "SchoolRole" },
                values: new object[] { true, false });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"),
                columns: new[] { "AdminRole", "SchoolRole" },
                values: new object[] { true, false });
        }
    }
}
