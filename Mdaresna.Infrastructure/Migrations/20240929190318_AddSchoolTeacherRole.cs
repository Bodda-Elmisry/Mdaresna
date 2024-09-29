using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolTeacherRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Active", "AdminRole", "CreateDate", "Description", "LastModifyDate", "Name", "SchoolRole" },
                values: new object[] { new Guid("10620c5f-37fe-4d18-996f-915ece8893f1"), true, true, null, "", null, "School Teacher", false });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId", "CreateDate", "LastModifyDate" },
                values: new object[] { new Guid("b15e32eb-092e-437d-9d4e-b9ed583c23b0"), new Guid("10620c5f-37fe-4d18-996f-915ece8893f1"), null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("b15e32eb-092e-437d-9d4e-b9ed583c23b0"), new Guid("10620c5f-37fe-4d18-996f-915ece8893f1") });

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("10620c5f-37fe-4d18-996f-915ece8893f1"));
        }
    }
}
