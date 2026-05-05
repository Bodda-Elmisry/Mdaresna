using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReportedUsersPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "AppPermission", "CreateDate", "Description", "Description_AR", "Key", "LastModifyDate", "Name", "Name_AR", "SchoolPermission" },
                values: new object[,]
                {
                    { new Guid("4c1e2d42-8f6c-4b8a-9a80-6a8f6d034d5a"), true, null, "View reported users with report counts", "عرض المستخدمين المبلغ عنهم مع عدد البلاغات", "ShowReportedUsers", null, "Show Reported Users", "عرض المستخدمين المبلغ عنهم", false },
                    { new Guid("ea7e7a5d-64c0-4ae6-b8b1-70a7d7e4f66c"), false, null, "View reported users for the selected school", "عرض المستخدمين المبلغ عنهم للمدرسة المحددة", "ShowSchoolReportedUsers", null, "Show School Reported Users", "عرض المستخدمين المبلغ عنهم للمدرسة", true }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId", "CreateDate", "LastModifyDate" },
                values: new object[,]
                {
                    { new Guid("4c1e2d42-8f6c-4b8a-9a80-6a8f6d034d5a"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1"), null, null },
                    { new Guid("ea7e7a5d-64c0-4ae6-b8b1-70a7d7e4f66c"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("4c1e2d42-8f6c-4b8a-9a80-6a8f6d034d5a"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("ea7e7a5d-64c0-4ae6-b8b1-70a7d7e4f66c"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("4c1e2d42-8f6c-4b8a-9a80-6a8f6d034d5a"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ea7e7a5d-64c0-4ae6-b8b1-70a7d7e4f66c"));
        }
    }
}
