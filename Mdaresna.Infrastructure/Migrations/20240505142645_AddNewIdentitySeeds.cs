using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNewIdentitySeeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "AppPermission", "Description", "Name", "SchoolPermission" },
                values: new object[,]
                {
                    { new Guid("219007ea-620e-4d96-8292-2d015ef68db1"), true, "View Childerns List", "ViewChildernsList", false },
                    { new Guid("9301fc37-ae75-4ef6-b6fa-a656452e5a2e"), true, "Create new shool", "CreateSchool", false }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { new Guid("9301fc37-ae75-4ef6-b6fa-a656452e5a2e"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") },
                    { new Guid("219007ea-620e-4d96-8292-2d015ef68db1"), new Guid("92d00b28-9d25-4bd2-a587-6c22a3a07a92") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("9301fc37-ae75-4ef6-b6fa-a656452e5a2e"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("219007ea-620e-4d96-8292-2d015ef68db1"), new Guid("92d00b28-9d25-4bd2-a587-6c22a3a07a92") });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("219007ea-620e-4d96-8292-2d015ef68db1"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("9301fc37-ae75-4ef6-b6fa-a656452e5a2e"));
        }
    }
}
