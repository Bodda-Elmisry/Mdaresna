using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addPaymentTypePermissionsSeedingData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "AppPermission", "CreateDate", "Description", "Description_AR", "Key", "LastModifyDate", "Name", "Name_AR", "SchoolPermission" },
                values: new object[,]
                {
                    { new Guid("0a0ab8a7-1d51-4b03-b10a-647a8f90ec24"), true, null, "Delete Payment Type", "حذف طريقة الدفع", "DeletePaymentType", null, "Delete Payment Type", "حذف طريقة الدفع", false },
                    { new Guid("56d9a5b6-28a6-4e8d-ade6-54ad37c846bd"), true, null, "Update Payment Type", "تعديل طريقة الدفع", "UpdatePaymentType", null, "Update Payment Type", "تعديل طريقة الدفع", false },
                    { new Guid("9c8526e9-9119-43d8-a434-4c4828c8a5d9"), true, null, "View Payment Types List", "عرض طرق الدفع", "ViewPaymentTypesList", null, "View Payment Types List", "عرض طرق الدفع", false }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("0a0ab8a7-1d51-4b03-b10a-647a8f90ec24"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("56d9a5b6-28a6-4e8d-ade6-54ad37c846bd"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("9c8526e9-9119-43d8-a434-4c4828c8a5d9"));
        }
    }
}
