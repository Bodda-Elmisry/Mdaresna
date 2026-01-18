using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addDeleteSchoolContactPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "AppPermission", "CreateDate", "Deleted", "Description", "Description_AR", "Key", "LastModifyDate", "Name", "Name_AR", "SchoolPermission" },
                values: new object[] { new Guid("2f14a9b5-3270-4391-ab66-0b09a59c460d"), false, null, true, "Delete School Contact", "حذف جهة اتصال المدرسة", "DeleteSchoolContact", null, "Delete School Contact", "حذف جهة اتصال المدرسة", true });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId", "CreateDate", "LastModifyDate" },
                values: new object[] { new Guid("2f14a9b5-3270-4391-ab66-0b09a59c460d"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("2f14a9b5-3270-4391-ab66-0b09a59c460d"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("2f14a9b5-3270-4391-ab66-0b09a59c460d"));
        }
    }
}
