using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addCoinTypePermissionsAndRolePermissionsSeedingData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "AppPermission", "CreateDate", "Description", "Description_AR", "Key", "LastModifyDate", "Name", "Name_AR", "SchoolPermission" },
                values: new object[,]
                {
                    { new Guid("b96d60ad-e4db-4ce9-8ea8-c22d2ce8c544"), true, null, "Delete Coin Type", "حذف نوع العمله", "DeleteCoinType", null, "Delete Coin Type", "حذف نوع العمله", false },
                    { new Guid("fbadcd2d-c9c8-4164-bad1-667a586b54cc"), true, null, "View Coin Types List", "عرض انواع العملات", "ViewCoinTypesList", null, "View Coin Types List", "عرض انواع العملات", false }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId", "CreateDate", "LastModifyDate" },
                values: new object[,]
                {
                    { new Guid("b96d60ad-e4db-4ce9-8ea8-c22d2ce8c544"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1"), null, null },
                    { new Guid("fbadcd2d-c9c8-4164-bad1-667a586b54cc"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1"), null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("b96d60ad-e4db-4ce9-8ea8-c22d2ce8c544"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("fbadcd2d-c9c8-4164-bad1-667a586b54cc"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1") });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b96d60ad-e4db-4ce9-8ea8-c22d2ce8c544"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("fbadcd2d-c9c8-4164-bad1-667a586b54cc"));
        }
    }
}
