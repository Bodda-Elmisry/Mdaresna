using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingSchoolPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "AppPermission", "CreateDate", "Description", "Key", "LastModifyDate", "Name", "SchoolPermission" },
                values: new object[,]
                {
                    { new Guid("1dfbdb17-ba97-4539-b887-e81fc0e72b47"), false, null, "View School Setting Section", "ViewSchoolSettingSection", null, "View School Setting Section", true },
                    { new Guid("4838711a-4139-465e-a34f-a4b6756ae475"), false, null, "View Years Setting", "ViewYearsSetting", null, "View Years Setting", true },
                    { new Guid("893e8a43-0da7-4149-abdb-e2469239896f"), false, null, "View Classes Setting Section", "ViewClassesSettingSection", null, "View Classes Setting Section", true },
                    { new Guid("8ad5e47c-5ec4-49c7-a0ab-0d37e576961f"), false, null, "View Year Setting Section", "ViewYearSettingSection", null, "View Year Setting Section", true },
                    { new Guid("97aad235-16fa-496b-88d2-adceefbd8d5c"), false, null, "school Admin", "schoolAdmin", null, "school Admin", true }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId", "CreateDate", "LastModifyDate" },
                values: new object[,]
                {
                    { new Guid("1dfbdb17-ba97-4539-b887-e81fc0e72b47"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("4838711a-4139-465e-a34f-a4b6756ae475"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("893e8a43-0da7-4149-abdb-e2469239896f"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("8ad5e47c-5ec4-49c7-a0ab-0d37e576961f"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("97aad235-16fa-496b-88d2-adceefbd8d5c"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("1dfbdb17-ba97-4539-b887-e81fc0e72b47"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("4838711a-4139-465e-a34f-a4b6756ae475"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("893e8a43-0da7-4149-abdb-e2469239896f"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("8ad5e47c-5ec4-49c7-a0ab-0d37e576961f"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("97aad235-16fa-496b-88d2-adceefbd8d5c"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("1dfbdb17-ba97-4539-b887-e81fc0e72b47"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("4838711a-4139-465e-a34f-a4b6756ae475"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("893e8a43-0da7-4149-abdb-e2469239896f"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("8ad5e47c-5ec4-49c7-a0ab-0d37e576961f"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("97aad235-16fa-496b-88d2-adceefbd8d5c"));
        }
    }
}
