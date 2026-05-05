using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangePaymentToAllocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("006edbb6-657d-41cf-9e01-8ab9cf4cea69"),
                columns: new[] { "Description", "Description_AR", "Name", "Name_AR" },
                values: new object[] { "Remove School Allocation", "إزالة وحدات مدرسية", "Remove School Allocation", "إزالة وحدات مدرسية" });
            
            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("061ac24b-3828-4360-833f-ef5865712e39"),
                columns: new[] { "Description", "Description_AR", "Name", "Name_AR" },
                values: new object[] { "Create Allocation Type", "إنشاء نوع تخصيص", "Create Allocation Type", "إنشاء نوع تخصيص" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("0a0ab8a7-1d51-4b03-b10a-647a8f90ec24"),
                columns: new[] { "Description", "Description_AR", "Name", "Name_AR" },
                values: new object[] { "Delete Allocation Type", "حذف طريقة التخصيص", "Delete Allocation Type", "حذف طريقة التهصيص" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("2989c158-ef37-40f8-bdf8-637c874f2f1e"),
                columns: new[] { "Description", "Description_AR", "Name", "Name_AR" },
                values: new object[] { "Add School Allocation", "إضافة وحدات مدرسية", "Add School Allocation", "إضافة وحدات مدرسية" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("512d2ebf-dd4a-482b-8753-1252fe196511"),
                columns: new[] { "Description", "Description_AR", "Name", "Name_AR" },
                values: new object[] { "View Allocations Transaction", "عرض حركات التخصيص", "View Allocations Transaction", "عرض حركات التخصيص" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("56d9a5b6-28a6-4e8d-ade6-54ad37c846bd"),
                columns: new[] { "Description", "Description_AR", "Name", "Name_AR" },
                values: new object[] { "Update Allocation Type", "تعديل طريقة التخصيص", "Update Allocation Type", "تعديل طريقة التخصيص" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("9c8526e9-9119-43d8-a434-4c4828c8a5d9"),
                columns: new[] { "Description", "Description_AR", "Name", "Name_AR" },
                values: new object[] { "View Allocation Types List", "عرض طرق التخصيص", "View Allocation Types List", "عرض طرق التخصيص" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a1bd5ef1-6a55-4e1e-8bb8-da39344e4412"),
                columns: new[] { "Description", "Description_AR", "Name", "Name_AR" },
                values: new object[] { "Delete School Allocations Request", "حذف طلب الوحدات المدرسة", "Delete School Allocations Request", "حذف طلب الوحدات المدرسة" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ebde1c53-4840-484f-ac23-df838e6282dc"),
                columns: new[] { "Description", "Description_AR", "Name", "Name_AR" },
                values: new object[] { "View Allocations Requests", "عرض طلبات التخصيص", "View Allocations Requests", "عرض طلبات التخصيص" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ec2c9457-2184-4b45-b271-a90a461a816e"),
                columns: new[] { "Description", "Description_AR", "Name", "Name_AR" },
                values: new object[] { "View School Allocations Request", "عرض طلب الوحدات المدرسية", "View School Allocations Request", "عرض طلب الوحدات المدرسية" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("f76d6cd8-4a98-4eba-9d09-9204009d7839"),
                columns: new[] { "Description", "Description_AR", "Name", "Name_AR" },
                values: new object[] { "Update School Allocation", "تحديث الوحدات المدرسية", "Update School Allocation", "تحديث الوحدات المدرسية" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("006edbb6-657d-41cf-9e01-8ab9cf4cea69"),
                columns: new[] { "Description", "Description_AR", "Name", "Name_AR" },
                values: new object[] { "Remove School Payment", "إزالة دفعة مدرسية", "Remove School Payment", "إزالة دفعة مدرسية" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("061ac24b-3828-4360-833f-ef5865712e39"),
                columns: new[] { "Description", "Description_AR", "Name", "Name_AR" },
                values: new object[] { "Create Payment Type", "إنشاء نوع دفع", "Create Payment Type", "إنشاء نوع دفع" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("0a0ab8a7-1d51-4b03-b10a-647a8f90ec24"),
                columns: new[] { "Description", "Description_AR", "Name", "Name_AR" },
                values: new object[] { "Delete Payment Type", "حذف طريقة الدفع", "Delete Payment Type", "حذف طريقة الدفع" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("2989c158-ef37-40f8-bdf8-637c874f2f1e"),
                columns: new[] { "Description", "Description_AR", "Name", "Name_AR" },
                values: new object[] { "Add School Payment", "إضافة دفعة مدرسية", "Add School Payment", "إضافة دفعة مدرسية" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("512d2ebf-dd4a-482b-8753-1252fe196511"),
                columns: new[] { "Description", "Description_AR", "Name", "Name_AR" },
                values: new object[] { "View Payments Transaction", "عرض حركات الدفع", "View Payments Transaction", "عرض حركات الدفع" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("56d9a5b6-28a6-4e8d-ade6-54ad37c846bd"),
                columns: new[] { "Description", "Description_AR", "Name", "Name_AR" },
                values: new object[] { "Update Payment Type", "تعديل طريقة الدفع", "Update Payment Type", "تعديل طريقة الدفع" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("9c8526e9-9119-43d8-a434-4c4828c8a5d9"),
                columns: new[] { "Description", "Description_AR", "Name", "Name_AR" },
                values: new object[] { "View Payment Types List", "عرض طرق الدفع", "View Payment Types List", "عرض طرق الدفع" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a1bd5ef1-6a55-4e1e-8bb8-da39344e4412"),
                columns: new[] { "Description", "Description_AR", "Name", "Name_AR" },
                values: new object[] { "Delete School Payment Request", "حذف طلب دفع المدرسة", "Delete School Payment Request", "حذف طلب دفع المدرسة" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ebde1c53-4840-484f-ac23-df838e6282dc"),
                columns: new[] { "Description", "Description_AR", "Name", "Name_AR" },
                values: new object[] { "View Payments Requests", "عرض طلبات الدفع", "View Payments Requests", "عرض طلبات الدفع" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ec2c9457-2184-4b45-b271-a90a461a816e"),
                columns: new[] { "Description", "Description_AR", "Name", "Name_AR" },
                values: new object[] { "View School Payment Request", "عرض طلب دفعة مدرسية", "View School Payment Request", "عرض طلب دفعة مدرسية" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("f76d6cd8-4a98-4eba-9d09-9204009d7839"),
                columns: new[] { "Description", "Description_AR", "Name", "Name_AR" },
                values: new object[] { "Update School Payment", "تحديث دفعة مدرسية", "Update School Payment", "تحديث دفعة مدرسية" });
        }
    }
}
