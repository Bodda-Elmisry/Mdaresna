using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultRolesToDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Active", "AdminRole", "Description", "Name", "SchoolRole" },
                values: new object[,]
                {
                    { new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1"), true, true, "This Role with fuly access to all Application featuers", "Application Owner", false },
                    { new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), true, true, "This Role with fuly access to all school featuers", "School Manager", false },
                    { new Guid("6e473a7a-083c-405b-8f0d-04dd85b94edb"), true, true, "This Role with fuly access to all Application featuers", "Application Manager", false },
                    { new Guid("92d00b28-9d25-4bd2-a587-6c22a3a07a92"), true, true, "This Role to work in normal account and it's the default role for all accounts. \nWith this role you can track your souns activites and schools.", "Standerd", false }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("6e473a7a-083c-405b-8f0d-04dd85b94edb"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("92d00b28-9d25-4bd2-a587-6c22a3a07a92"));
        }
    }
}
