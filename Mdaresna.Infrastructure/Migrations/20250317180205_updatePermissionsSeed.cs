using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updatePermissionsSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("69a0778b-a7a8-4e47-b8fd-d061428dbb95"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1") });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("69a0778b-a7a8-4e47-b8fd-d061428dbb95"),
                columns: new[] { "AppPermission", "SchoolPermission" },
                values: new object[] { false, true });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId", "CreateDate", "LastModifyDate" },
                values: new object[] { new Guid("69a0778b-a7a8-4e47-b8fd-d061428dbb95"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("69a0778b-a7a8-4e47-b8fd-d061428dbb95"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("69a0778b-a7a8-4e47-b8fd-d061428dbb95"),
                columns: new[] { "AppPermission", "SchoolPermission" },
                values: new object[] { true, false });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId", "CreateDate", "LastModifyDate" },
                values: new object[] { new Guid("69a0778b-a7a8-4e47-b8fd-d061428dbb95"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1"), null, null });
        }
    }
}
