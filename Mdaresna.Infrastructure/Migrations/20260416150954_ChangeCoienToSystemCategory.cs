using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeCoienToSystemCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("76befb0e-5ef7-4fe3-8e06-02d50c17c38b"),
                columns: new[] { "Description", "Description_AR", "Name", "Name_AR" },
                values: new object[] { "Change School system category Type", "تغيير نوع فئة النظام المدرسة", "Change School system category Type", "تغيير نوع فئة النظام المدرسة" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("8a48610d-c9ff-4fce-95eb-3a4d8d633a9c"),
                columns: new[] { "Description", "Description_AR", "Name", "Name_AR" },
                values: new object[] { "Assign system catogries To School", "تعيين فئة النظام للمدرسة", "Assign system catogries To School", "تعيين فئة النظام للمدرسة" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b96d60ad-e4db-4ce9-8ea8-c22d2ce8c544"),
                columns: new[] { "Description", "Description_AR", "Name", "Name_AR" },
                values: new object[] { "Delete system category Type", "حذف نوع فئة النظام", "Delete system category Type", "حذف نوع فئة النظام" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d330df2e-9b66-46a0-a64d-88b1d4f9519d"),
                columns: new[] { "Description", "Description_AR", "Name", "Name_AR" },
                values: new object[] { "Edit system category Type", "تعديل نوع فئة النظام", "Edit system category Type", "تعديل نوع فئة النظام" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d9bb6050-186d-4052-a3dc-6d33a84424ac"),
                columns: new[] { "Description", "Description_AR", "Name", "Name_AR" },
                values: new object[] { "Create system category Type", "إنشاء نوع فئة نظام", "Create system category Type", "إنشاء نوع فئة نظام" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("fbadcd2d-c9c8-4164-bad1-667a586b54cc"),
                columns: new[] { "Description", "Description_AR", "Name", "Name_AR" },
                values: new object[] { "View system category Types List", "عرض انواع فئة النظام", "View system category Types List", "عرض انواع فئة النظام" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("76befb0e-5ef7-4fe3-8e06-02d50c17c38b"),
                columns: new[] { "Description", "Description_AR", "Name", "Name_AR" },
                values: new object[] { "Change School Coin Type", "تغيير نوع عملة المدرسة", "Change School Coin Type", "تغيير نوع عملة المدرسة" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("8a48610d-c9ff-4fce-95eb-3a4d8d633a9c"),
                columns: new[] { "Description", "Description_AR", "Name", "Name_AR" },
                values: new object[] { "Assign Coins To School", "تعيين العملات للمدرسة", "Assign Coins To School", "تعيين العملات للمدرسة" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b96d60ad-e4db-4ce9-8ea8-c22d2ce8c544"),
                columns: new[] { "Description", "Description_AR", "Name", "Name_AR" },
                values: new object[] { "Delete Coin Type", "حذف نوع العمله", "Delete Coin Type", "حذف نوع العمله" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d330df2e-9b66-46a0-a64d-88b1d4f9519d"),
                columns: new[] { "Description", "Description_AR", "Name", "Name_AR" },
                values: new object[] { "Edit Coin Type", "تعديل نوع العملة", "Edit Coin Type", "تعديل نوع العملة" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d9bb6050-186d-4052-a3dc-6d33a84424ac"),
                columns: new[] { "Description", "Description_AR", "Name", "Name_AR" },
                values: new object[] { "Create Coin Type", "إنشاء نوع عملة", "Create Coin Type", "إنشاء نوع عملة" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("fbadcd2d-c9c8-4164-bad1-667a586b54cc"),
                columns: new[] { "Description", "Description_AR", "Name", "Name_AR" },
                values: new object[] { "View Coin Types List", "عرض انواع العملات", "View Coin Types List", "عرض انواع العملات" });
        }
    }
}
