using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddManageSchoolRolePermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "AppPermission", "AvailableForSchoolCustomRoles", "CreateDate", "Description", "Description_AR", "Key", "LastModifyDate", "Name", "Name_AR", "SchoolPermission" },
                values: new object[] { new Guid("e5f6a7b8-c9d0-4123-e456-789abc012345"), false, false, null, "Add or remove permissions from custom school roles", "إضافة أو إزالة الصلاحيات من أدوار المدرسة المخصصة", "ManageSchoolRolePermissions", null, "Manage School Role Permissions", "إدارة صلاحيات أدوار المدرسة", true });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId", "CreateDate", "LastModifyDate" },
                values: new object[] { new Guid("e5f6a7b8-c9d0-4123-e456-789abc012345"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("e5f6a7b8-c9d0-4123-e456-789abc012345"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("e5f6a7b8-c9d0-4123-e456-789abc012345"));
        }
    }
}
