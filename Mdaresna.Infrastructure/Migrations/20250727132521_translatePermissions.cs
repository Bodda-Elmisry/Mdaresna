using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class translatePermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("512d2ebf-dd4a-482b-8753-1252fe196511"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "عرض حركات الدفع", "عرض حركات الدفع" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a8704499-413d-4ff4-a3a2-122b684a0e17"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "عرض اعدادات الصلاحيات", "عرض اعدادات الصلاحيات" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("512d2ebf-dd4a-482b-8753-1252fe196511"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "", "" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a8704499-413d-4ff4-a3a2-122b684a0e17"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "", "" });
        }
    }
}
