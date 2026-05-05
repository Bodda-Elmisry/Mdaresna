using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateViewYearMontsPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("69a0778b-a7a8-4e47-b8fd-d061428dbb95"),
                columns: new[] { "Description", "Description_AR", "Name", "Name_AR" },
                values: new object[] { "manage Year Monthes", "ادارة أشهر السنة الدراسية عرض, اضافة, تعديل وحذف", "Manage Year Monthes", "ادارة أشهر السنة الدراسية عرض, اضافة, تعديل وحذف" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("69a0778b-a7a8-4e47-b8fd-d061428dbb95"),
                columns: new[] { "Description", "Description_AR", "Name", "Name_AR" },
                values: new object[] { "View Year Monthes", "عرض أشهر السنة الدراسية", "View Year Monthes", "عرض أشهر السنة الدراسية" });
        }
    }
}
