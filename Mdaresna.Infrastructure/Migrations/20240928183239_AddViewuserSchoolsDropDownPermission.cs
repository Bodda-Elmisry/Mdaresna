using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddViewuserSchoolsDropDownPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "AppPermission", "CreateDate", "Description", "Key", "LastModifyDate", "Name", "SchoolPermission" },
                values: new object[] { new Guid("b15e32eb-092e-437d-9d4e-b9ed583c23b0"), true, null, "View User Schools Drop Down", "ViewUserSchoolsDropDown", null, "View User Schools Drop Down", false });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId", "CreateDate", "LastModifyDate" },
                values: new object[] { new Guid("b15e32eb-092e-437d-9d4e-b9ed583c23b0"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("b15e32eb-092e-437d-9d4e-b9ed583c23b0"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b15e32eb-092e-437d-9d4e-b9ed583c23b0"));
        }
    }
}
