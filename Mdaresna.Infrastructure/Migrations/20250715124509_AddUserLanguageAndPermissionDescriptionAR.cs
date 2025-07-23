using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserLanguageAndPermissionDescriptionAR : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DemoAccount",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description_AR",
                table: "Permissions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name_AR",
                table: "Permissions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("0225dbd5-9675-438c-87f2-63fb6921841c"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "تعيين صلاحية مدير المدرسة للمستخدم", "تعيين مدير المدرسة للمستخدم" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("056ed8bb-3ffc-459f-831e-c8a462db313b"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "عرض قائمة إعدادات الفصول", "عرض قائمة إعدادات الفصول" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("062ff041-c775-4b30-8a49-202db7d9dd28"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "إضافة امتحانات متعددة", "إضافة امتحانات متعددة" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("072c7a82-b62d-45d8-a3ed-48a70319abf3"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "تغيير تفعيل المدرسة", "تغيير تفعيل المدرسة" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("09e73f16-bb7a-403b-a8f9-fdc1c3228b0d"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "عرض إعدادات المدرسة", "عرض إعدادات المدرسة" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("0d9bfad2-0762-47ba-8046-3aa92b2d6222"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "إضافة مستويات دراسية متعددة", "إضافة مستويات دراسية متعددة" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("11f0aa92-e7af-4f94-bd50-96f389deee2a"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "تغيير تفعيل الشهر الدراسي", "تغيير تفعيل الشهر الدراسي" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("122d8e7a-f5a7-48ea-b2a6-f86964a2c3e7"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "تغيير نوع المدرسة", "تغيير نوع المدرسة" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("16983650-564e-4331-97d2-c1b5d67fef40"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "تحديث جهة اتصال المدرسة", "تحديث جهة اتصال المدرسة" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("1dfbdb17-ba97-4539-b887-e81fc0e72b47"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "عرض قسم إعدادات المدرسة", "عرض قسم إعدادات المدرسة" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("21040e37-d949-42ea-9a77-96aed0289209"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "تحديث بيانات المدرسة", "تحديث بيانات المدرسة" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("219007ea-620e-4d96-8292-2d015ef68db1"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "عرض قائمة الأطفال", "عرض قائمة الأطفال" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("26de4395-48fb-4684-b88d-5c9e6081f9ac"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "إضافة جهة اتصال للمدرسة", "إضافة جهة اتصال للمدرسة" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("2f1290b1-f439-4810-ba25-5c3e9cc56efe"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "تحديث معلومات الطالب", "تحديث معلومات الطالب" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("30aa5979-36f6-4a63-98d2-a06f96178176"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "إضافة واجب منزلي", "إضافة واجب منزلي" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("32d821bb-0c50-4721-9034-097019632c05"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "إضافة شهر دراسي", "إضافة شهر دراسي" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("3b49fc12-d345-41c4-b5d0-97a388aaafa3"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "إجراء الحضور والغياب", "إجراء الحضور والغياب" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("4838711a-4139-465e-a34f-a4b6756ae475"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "عرض إعدادات السنوات", "عرض إعدادات السنوات" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("494d9c56-558a-427c-9854-878520fcdec8"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "عرض إعدادات الامتحانات", "عرض إعدادات الامتحانات" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("49869e47-2209-4fd8-aa08-2e99ba211ff1"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "عرض الأنشطة", "عرض الأنشطة" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("5c8d7610-bfaa-4deb-bd84-e54ed4351611"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "إضافة مستوى دراسي", "إضافة مستوى دراسي" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("65b57f13-161f-43b2-9e8d-daafb50fbbbb"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "تحديث الواجب المنزلي", "تحديث الواجب المنزلي" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("68713df9-ed1b-449c-8060-d919a2592b02"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "إنشاء لغة", "إنشاء لغة" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("69a0778b-a7a8-4e47-b8fd-d061428dbb95"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "عرض أشهر السنة الدراسية", "عرض أشهر السنة الدراسية" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("6de52188-1c80-46e3-a436-ff36469e2976"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "عرض إعدادات المسؤولين", "عرض إعدادات المسؤولين" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("6ed86695-9ea7-4820-bf89-0de16dfd6ea9"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "إضافة مواد دراسية متعددة", "إضافة مواد دراسية متعددة" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("71b6f158-82f2-4a48-9950-56dbb00eec4e"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "عرض اللغات", "عرض اللغات" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("75162229-2536-4232-b081-feabe20c318d"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "عرض إعدادات المستويات الدراسة", "عرض إعدادات المستويات الدراسية" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("76befb0e-5ef7-4fe3-8e06-02d50c17c38b"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "تغيير نوع عملة المدرسة", "تغيير نوع عملة المدرسة" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("77127635-8fe8-4451-ac9c-59c38ee02d4d"),
                column: "Description_AR",
                value: null);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("7a382b52-95f3-442f-abff-9c1c5c9607cc"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "تحديث الفصل الدراسي", "تحديث الفصل الدراسي" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("7b4478c8-2526-493a-a34d-9fffa5786f85"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "عرض قائمة المعلمين", "عرض قائمة المعلمين" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("84956c26-e46f-432a-9b44-c8fb17e95552"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "إضافة فصول دراسية متعددة", "إضافة فصول دراسية متعددة" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("861ad498-a214-4b7d-bde7-815abf63a587"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "إضافة امتحان", "إضافة امتحان" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("869a6521-2730-4f0b-973b-60fb8093c769"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "فك ارتباط المعلم بالمادة الدراسية والفصل", "فك ارتباط المعلم بالمادة الدراسية والفصل" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("893e8a43-0da7-4149-abdb-e2469239896f"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "عرض قسم إعدادات الفصول", "عرض قسم إعدادات الفصول" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("8a48610d-c9ff-4fce-95eb-3a4d8d633a9c"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "تعيين العملات للمدرسة", "تعيين العملات للمدرسة" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("8ad5e47c-5ec4-49c7-a0ab-0d37e576961f"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "عرض قسم إعدادات السنة", "عرض قسم إعدادات السنة" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("9301fc37-ae75-4ef6-b6fa-a656452e5a2e"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "إنشاء مدرسة جديدة", "إضافة مدرسة" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("93fd0aa6-852f-4afe-8cf4-9cdeb365d498"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "تحديث الشهر الدراسي", "تحديث الشهر الدراسي" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("97aad235-16fa-496b-88d2-adceefbd8d5c"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "مدير المدرسة", "مدير المدرسة" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("98d37ade-fdc0-4a0b-8e00-402a708c9ddd"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "ربط المعلم بالمادة الدراسية والفصل", "ربط المعلم بالمادة الدراسية والفصل" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("99fdfba4-ef5b-44d1-b9bf-b4faf68a7b60"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "تقييم النشاط", "تقييم النشاط" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("9dd22e15-9701-492b-ae20-985b8927f3bf"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "تغيير تفعيل الطالب", "تغيير تفعيل الطالب" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a1134102-7021-4770-8808-fdc376190691"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "تحديث المستوى الدراسي", "تحديث المستوى الدراسي" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a4a93ceb-21f9-4400-9444-0d94cf895beb"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "تحديث المادة الدراسية", "تحديث المادة الدراسية" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a992e74b-4ce4-4d18-bcc5-c079d116d1f5"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "تحديث النشاط", "تحديث النشاط" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ad53e402-c2ab-4941-861b-a19c82dca0cc"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "تعيين طالب للمدرسة", "تعيين طالب للمدرسة" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b15e32eb-092e-437d-9d4e-b9ed583c23b0"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "عرض قائمة المدارس للمستخدم", "عرض قائمة المدارس للمستخدم" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b65324a3-35b9-4579-8c85-c9938391dfa6"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "تعيين طلاب متعددين للمدرسة", "تعيين طلاب متعددين للمدرسة" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b6858fe8-dcc3-4c0f-b1a9-6a8ab7331924"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "ربط المعلم بالمدرسة", "ربط المعلم بالمدرسة" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b6d6c7e2-e2de-4b95-9789-a61c69f27765"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "تغيير تفعيل السنة الدراسية", "تغيير تفعيل السنة الدراسية" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b7bcf498-f7e9-48b4-9e99-49f6d0a9a956"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "تعديل الملاحظة", "تعديل الملاحظة" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b7f4d3f3-b0b9-4e2f-8884-e2d61e0b6f5e"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "رفع صورة المدرسة", "رفع صورة المدرسة" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("baa954fd-8ba9-4529-834d-e640057998af"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "عرض الواجبات المنزلية", "عرض الواجبات المنزلية" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("be86c901-37f9-4f63-b7b3-03653e75fea5"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "إضافة لغة للمدرسة", "إضافة لغة للمدرسة" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("c1856505-7c6b-4932-8034-cf4fbc0efb67"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "تقييم الواجب المنزلي", "تقييم الواجب المنزلي" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("c50d6574-648b-4f6e-8ed5-d27f8b5a34f1"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "إضافة سنة دراسية جديدة", "إضافة سنة دراسية جديدة" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("cb37bbd0-40ad-4ca8-ae19-1ed87fea0b97"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "إضافة فصل دراسي", "إضافة فصل دراسي" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("cc4a8f98-fbb7-49dd-8b5f-cc6d02bccab0"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "عرض السنة الدراسية الحالية للمدرسة", "عرض السنة الدراسية الحالية للمدرسة" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d512c1b3-dde0-4646-98cc-2be5c56c7150"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "عرض إعدادات لغات المدرسة", "عرض إعدادات لغات المدرسة" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d9b2c724-c5cc-497a-a11b-3e48b7dbfd97"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "تحديث السنة الدراسية", "تحديث السنة الدراسية" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d9bdcf84-d193-4775-8f9b-bacb1bd22915"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "عرض الملاحظات", "عرض الملاحظات" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("dd0f56ad-6039-4a8b-9d26-ebd5abe87e7d"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "إضافة مادة دراسية", "إضافة مادة دراسية" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("de53cf00-e53d-4be0-8f67-71f8c7248df4"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "عرض قائمة الحضور والغياب", "عرض قائمة الحضور والغياب" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("e09a3a9f-0ff1-46b7-8c80-b7f07ccb4542"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "إضافة نشاط", "إضافة نشاط" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("e50a5830-e741-4260-820c-19c2cab1b419"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "عرض بيانات المدرسة", "عرض بيانات المدرسة" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("e5b8454f-a193-4d63-b791-3a02aa9d71d3"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "عرض قسم إعدادات الفصول", "عرض قسم إعدادات الفصول" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("e831e41d-1090-4ad0-bbaf-e4ca823ecd9d"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "عرض قائمة السنوات الدراسية", "عرض قائمة السنوات الدراسية" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ebd73c8f-2ef9-4a67-99e6-bafe71c9e83c"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "تقييم الامتحان", "تقييم الامتحان" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ecd2236f-6fa1-43ec-92e9-aa36ae2358b4"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "إكمال السنة الدراسية", "إكمال السنة الدراسية" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ee6325a4-4bc4-4aa7-9944-f82c3b7a305a"),
                column: "Description_AR",
                value: null);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("eec01fe0-4399-44b0-b8e7-afd417a6a93c"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "إضافة ملاحظة", "إضافة ملاحظة" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("f19ccd22-35ac-4778-8a7f-fb0e56822385"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "تغيير تفعيل الفصل الدراسي", "تغيير تفعيل الفصل الدراسي" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("fba69a66-4e31-44f2-93ac-9602705a7f98"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "عرض الامتحانات", "عرض الامتحانات" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("fdebaade-4d2c-447b-ba52-11e3e2a622f4"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "تصفية المدارس حسب النشاط", "تصفية المدارس حسب النشاط" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("fe286e0a-193b-4f12-ae88-df5808fb2eb7"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "فك ارتباط المعلم بالمدرسة", "فك ارتباط المعلم بالمدرسة" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("d4c7be15-c9b6-4d83-8516-aff52c94f963"),
                columns: new[] { "DemoAccount", "Language" },
                values: new object[] { true, "en" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("de36f342-fe3c-46c3-bdfc-bb3fcf2ec7e4"),
                columns: new[] { "DemoAccount", "Language" },
                values: new object[] { true, "en" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DemoAccount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Description_AR",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "Name_AR",
                table: "Permissions");
        }
    }
}
