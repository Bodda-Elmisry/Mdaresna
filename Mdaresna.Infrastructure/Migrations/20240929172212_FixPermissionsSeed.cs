using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixPermissionsSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("da3e8df0-4d2b-412a-82d5-3b8bcf62d827"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("da3e8df0-4d2b-412a-82d5-3b8bcf62d827"));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("062ff041-c775-4b30-8a49-202db7d9dd28"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("09e73f16-bb7a-403b-a8f9-fdc1c3228b0d"),
                columns: new[] { "AppPermission", "Description", "Key", "Name" },
                values: new object[] { true, "View School Settings", "ViewSchoolSettings", "View School Settings" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("0d9bfad2-0762-47ba-8046-3aa92b2d6222"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("11f0aa92-e7af-4f94-bd50-96f389deee2a"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("16983650-564e-4331-97d2-c1b5d67fef40"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("26de4395-48fb-4684-b88d-5c9e6081f9ac"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("2f1290b1-f439-4810-ba25-5c3e9cc56efe"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("30aa5979-36f6-4a63-98d2-a06f96178176"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("32d821bb-0c50-4721-9034-097019632c05"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("3b49fc12-d345-41c4-b5d0-97a388aaafa3"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("494d9c56-558a-427c-9854-878520fcdec8"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("49869e47-2209-4fd8-aa08-2e99ba211ff1"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("5c8d7610-bfaa-4deb-bd84-e54ed4351611"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("65b57f13-161f-43b2-9e8d-daafb50fbbbb"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("6de52188-1c80-46e3-a436-ff36469e2976"),
                columns: new[] { "AppPermission", "Description", "Key", "Name", "SchoolPermission" },
                values: new object[] { true, "View Admin Aettings", "ViewAdminSettings", "View Admin Aettings", false });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("6ed86695-9ea7-4820-bf89-0de16dfd6ea9"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("75162229-2536-4232-b081-feabe20c318d"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("77127635-8fe8-4451-ac9c-59c38ee02d4d"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("7a382b52-95f3-442f-abff-9c1c5c9607cc"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("7b4478c8-2526-493a-a34d-9fffa5786f85"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("84956c26-e46f-432a-9b44-c8fb17e95552"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("861ad498-a214-4b7d-bde7-815abf63a587"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("869a6521-2730-4f0b-973b-60fb8093c769"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("9301fc37-ae75-4ef6-b6fa-a656452e5a2e"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("93fd0aa6-852f-4afe-8cf4-9cdeb365d498"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("98d37ade-fdc0-4a0b-8e00-402a708c9ddd"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("99fdfba4-ef5b-44d1-b9bf-b4faf68a7b60"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("9dd22e15-9701-492b-ae20-985b8927f3bf"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a1134102-7021-4770-8808-fdc376190691"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a4a93ceb-21f9-4400-9444-0d94cf895beb"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a992e74b-4ce4-4d18-bcc5-c079d116d1f5"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ad53e402-c2ab-4941-861b-a19c82dca0cc"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b65324a3-35b9-4579-8c85-c9938391dfa6"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b6858fe8-dcc3-4c0f-b1a9-6a8ab7331924"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b6d6c7e2-e2de-4b95-9789-a61c69f27765"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b7bcf498-f7e9-48b4-9e99-49f6d0a9a956"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b7f4d3f3-b0b9-4e2f-8884-e2d61e0b6f5e"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("baa954fd-8ba9-4529-834d-e640057998af"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("c50d6574-648b-4f6e-8ed5-d27f8b5a34f1"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("cb37bbd0-40ad-4ca8-ae19-1ed87fea0b97"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("cc4a8f98-fbb7-49dd-8b5f-cc6d02bccab0"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d512c1b3-dde0-4646-98cc-2be5c56c7150"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d9b2c724-c5cc-497a-a11b-3e48b7dbfd97"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d9bdcf84-d193-4775-8f9b-bacb1bd22915"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("dd0f56ad-6039-4a8b-9d26-ebd5abe87e7d"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("de53cf00-e53d-4be0-8f67-71f8c7248df4"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("e09a3a9f-0ff1-46b7-8c80-b7f07ccb4542"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("e50a5830-e741-4260-820c-19c2cab1b419"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("e5b8454f-a193-4d63-b791-3a02aa9d71d3"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("e831e41d-1090-4ad0-bbaf-e4ca823ecd9d"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ebd73c8f-2ef9-4a67-99e6-bafe71c9e83c"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ecd2236f-6fa1-43ec-92e9-aa36ae2358b4"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ee6325a4-4bc4-4aa7-9944-f82c3b7a305a"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("eec01fe0-4399-44b0-b8e7-afd417a6a93c"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("f19ccd22-35ac-4778-8a7f-fb0e56822385"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("fba69a66-4e31-44f2-93ac-9602705a7f98"),
                column: "AppPermission",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("fe286e0a-193b-4f12-ae88-df5808fb2eb7"),
                column: "AppPermission",
                value: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("062ff041-c775-4b30-8a49-202db7d9dd28"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("09e73f16-bb7a-403b-a8f9-fdc1c3228b0d"),
                columns: new[] { "AppPermission", "Description", "Key", "Name" },
                values: new object[] { false, "Add Exam", "AddExam", "Add Exam" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("0d9bfad2-0762-47ba-8046-3aa92b2d6222"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("11f0aa92-e7af-4f94-bd50-96f389deee2a"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("16983650-564e-4331-97d2-c1b5d67fef40"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("26de4395-48fb-4684-b88d-5c9e6081f9ac"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("2f1290b1-f439-4810-ba25-5c3e9cc56efe"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("30aa5979-36f6-4a63-98d2-a06f96178176"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("32d821bb-0c50-4721-9034-097019632c05"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("3b49fc12-d345-41c4-b5d0-97a388aaafa3"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("494d9c56-558a-427c-9854-878520fcdec8"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("49869e47-2209-4fd8-aa08-2e99ba211ff1"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("5c8d7610-bfaa-4deb-bd84-e54ed4351611"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("65b57f13-161f-43b2-9e8d-daafb50fbbbb"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("6de52188-1c80-46e3-a436-ff36469e2976"),
                columns: new[] { "AppPermission", "Description", "Key", "Name", "SchoolPermission" },
                values: new object[] { false, "Link Teacher To School", "LinkTeacherToSchool", "Link Teacher To School", true });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("6ed86695-9ea7-4820-bf89-0de16dfd6ea9"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("75162229-2536-4232-b081-feabe20c318d"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("77127635-8fe8-4451-ac9c-59c38ee02d4d"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("7a382b52-95f3-442f-abff-9c1c5c9607cc"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("7b4478c8-2526-493a-a34d-9fffa5786f85"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("84956c26-e46f-432a-9b44-c8fb17e95552"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("861ad498-a214-4b7d-bde7-815abf63a587"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("869a6521-2730-4f0b-973b-60fb8093c769"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("9301fc37-ae75-4ef6-b6fa-a656452e5a2e"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("93fd0aa6-852f-4afe-8cf4-9cdeb365d498"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("98d37ade-fdc0-4a0b-8e00-402a708c9ddd"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("99fdfba4-ef5b-44d1-b9bf-b4faf68a7b60"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("9dd22e15-9701-492b-ae20-985b8927f3bf"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a1134102-7021-4770-8808-fdc376190691"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a4a93ceb-21f9-4400-9444-0d94cf895beb"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a992e74b-4ce4-4d18-bcc5-c079d116d1f5"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ad53e402-c2ab-4941-861b-a19c82dca0cc"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b65324a3-35b9-4579-8c85-c9938391dfa6"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b6858fe8-dcc3-4c0f-b1a9-6a8ab7331924"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b6d6c7e2-e2de-4b95-9789-a61c69f27765"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b7bcf498-f7e9-48b4-9e99-49f6d0a9a956"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b7f4d3f3-b0b9-4e2f-8884-e2d61e0b6f5e"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("baa954fd-8ba9-4529-834d-e640057998af"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("c50d6574-648b-4f6e-8ed5-d27f8b5a34f1"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("cb37bbd0-40ad-4ca8-ae19-1ed87fea0b97"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("cc4a8f98-fbb7-49dd-8b5f-cc6d02bccab0"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d512c1b3-dde0-4646-98cc-2be5c56c7150"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d9b2c724-c5cc-497a-a11b-3e48b7dbfd97"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d9bdcf84-d193-4775-8f9b-bacb1bd22915"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("dd0f56ad-6039-4a8b-9d26-ebd5abe87e7d"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("de53cf00-e53d-4be0-8f67-71f8c7248df4"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("e09a3a9f-0ff1-46b7-8c80-b7f07ccb4542"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("e50a5830-e741-4260-820c-19c2cab1b419"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("e5b8454f-a193-4d63-b791-3a02aa9d71d3"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("e831e41d-1090-4ad0-bbaf-e4ca823ecd9d"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ebd73c8f-2ef9-4a67-99e6-bafe71c9e83c"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ecd2236f-6fa1-43ec-92e9-aa36ae2358b4"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ee6325a4-4bc4-4aa7-9944-f82c3b7a305a"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("eec01fe0-4399-44b0-b8e7-afd417a6a93c"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("f19ccd22-35ac-4778-8a7f-fb0e56822385"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("fba69a66-4e31-44f2-93ac-9602705a7f98"),
                column: "AppPermission",
                value: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("fe286e0a-193b-4f12-ae88-df5808fb2eb7"),
                column: "AppPermission",
                value: false);

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "AppPermission", "CreateDate", "Description", "Key", "LastModifyDate", "Name", "SchoolPermission" },
                values: new object[] { new Guid("da3e8df0-4d2b-412a-82d5-3b8bcf62d827"), false, null, "Update Exam", "UpdateExam", null, "Update Exam", true });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId", "CreateDate", "LastModifyDate" },
                values: new object[] { new Guid("da3e8df0-4d2b-412a-82d5-3b8bcf62d827"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null });
        }
    }
}
