using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDataSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("6e473a7a-083c-405b-8f0d-04dd85b94edb"));

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId", "CreateDate", "LastModifyDate" },
                values: new object[,]
                {
                    { new Guid("0225dbd5-9675-438c-87f2-63fb6921841c"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1"), null, null },
                    { new Guid("072c7a82-b62d-45d8-a3ed-48a70319abf3"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1"), null, null },
                    { new Guid("122d8e7a-f5a7-48ea-b2a6-f86964a2c3e7"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1"), null, null },
                    { new Guid("219007ea-620e-4d96-8292-2d015ef68db1"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1"), null, null },
                    { new Guid("68713df9-ed1b-449c-8060-d919a2592b02"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1"), null, null },
                    { new Guid("69a0778b-a7a8-4e47-b8fd-d061428dbb95"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1"), null, null },
                    { new Guid("71b6f158-82f2-4a48-9950-56dbb00eec4e"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1"), null, null },
                    { new Guid("76befb0e-5ef7-4fe3-8e06-02d50c17c38b"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1"), null, null },
                    { new Guid("8a48610d-c9ff-4fce-95eb-3a4d8d633a9c"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1"), null, null },
                    { new Guid("fdebaade-4d2c-447b-ba52-11e3e2a622f4"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1"), null, null },
                    { new Guid("056ed8bb-3ffc-459f-831e-c8a462db313b"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("062ff041-c775-4b30-8a49-202db7d9dd28"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("09e73f16-bb7a-403b-a8f9-fdc1c3228b0d"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("0d9bfad2-0762-47ba-8046-3aa92b2d6222"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("11f0aa92-e7af-4f94-bd50-96f389deee2a"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("16983650-564e-4331-97d2-c1b5d67fef40"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("21040e37-d949-42ea-9a77-96aed0289209"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("26de4395-48fb-4684-b88d-5c9e6081f9ac"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("2f1290b1-f439-4810-ba25-5c3e9cc56efe"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("30aa5979-36f6-4a63-98d2-a06f96178176"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("32d821bb-0c50-4721-9034-097019632c05"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("3b49fc12-d345-41c4-b5d0-97a388aaafa3"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("494d9c56-558a-427c-9854-878520fcdec8"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("49869e47-2209-4fd8-aa08-2e99ba211ff1"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("5c8d7610-bfaa-4deb-bd84-e54ed4351611"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("65b57f13-161f-43b2-9e8d-daafb50fbbbb"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("6de52188-1c80-46e3-a436-ff36469e2976"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("6ed86695-9ea7-4820-bf89-0de16dfd6ea9"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("75162229-2536-4232-b081-feabe20c318d"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("77127635-8fe8-4451-ac9c-59c38ee02d4d"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("7a382b52-95f3-442f-abff-9c1c5c9607cc"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("7b4478c8-2526-493a-a34d-9fffa5786f85"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("84956c26-e46f-432a-9b44-c8fb17e95552"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("861ad498-a214-4b7d-bde7-815abf63a587"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("869a6521-2730-4f0b-973b-60fb8093c769"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("93fd0aa6-852f-4afe-8cf4-9cdeb365d498"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("98d37ade-fdc0-4a0b-8e00-402a708c9ddd"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("99fdfba4-ef5b-44d1-b9bf-b4faf68a7b60"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("9dd22e15-9701-492b-ae20-985b8927f3bf"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("a1134102-7021-4770-8808-fdc376190691"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("a4a93ceb-21f9-4400-9444-0d94cf895beb"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("a992e74b-4ce4-4d18-bcc5-c079d116d1f5"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("ad53e402-c2ab-4941-861b-a19c82dca0cc"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("b65324a3-35b9-4579-8c85-c9938391dfa6"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("b6858fe8-dcc3-4c0f-b1a9-6a8ab7331924"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("b6d6c7e2-e2de-4b95-9789-a61c69f27765"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("b7bcf498-f7e9-48b4-9e99-49f6d0a9a956"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("b7f4d3f3-b0b9-4e2f-8884-e2d61e0b6f5e"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("baa954fd-8ba9-4529-834d-e640057998af"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("be86c901-37f9-4f63-b7b3-03653e75fea5"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("c1856505-7c6b-4932-8034-cf4fbc0efb67"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("c50d6574-648b-4f6e-8ed5-d27f8b5a34f1"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("cb37bbd0-40ad-4ca8-ae19-1ed87fea0b97"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("cc4a8f98-fbb7-49dd-8b5f-cc6d02bccab0"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("d512c1b3-dde0-4646-98cc-2be5c56c7150"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("d9b2c724-c5cc-497a-a11b-3e48b7dbfd97"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("d9bdcf84-d193-4775-8f9b-bacb1bd22915"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("da3e8df0-4d2b-412a-82d5-3b8bcf62d827"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("dd0f56ad-6039-4a8b-9d26-ebd5abe87e7d"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("de53cf00-e53d-4be0-8f67-71f8c7248df4"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("e09a3a9f-0ff1-46b7-8c80-b7f07ccb4542"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("e50a5830-e741-4260-820c-19c2cab1b419"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("e5b8454f-a193-4d63-b791-3a02aa9d71d3"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("e831e41d-1090-4ad0-bbaf-e4ca823ecd9d"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("ebd73c8f-2ef9-4a67-99e6-bafe71c9e83c"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("ecd2236f-6fa1-43ec-92e9-aa36ae2358b4"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("ee6325a4-4bc4-4aa7-9944-f82c3b7a305a"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("eec01fe0-4399-44b0-b8e7-afd417a6a93c"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("f19ccd22-35ac-4778-8a7f-fb0e56822385"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("fba69a66-4e31-44f2-93ac-9602705a7f98"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("fdebaade-4d2c-447b-ba52-11e3e2a622f4"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("fe286e0a-193b-4f12-ae88-df5808fb2eb7"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null }
                });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1"),
                column: "Name",
                value: "Application Manager");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Address", "BirthDay", "City", "Contry", "CreateDate", "Email", "EmailConfirmationKey", "EmailConfirmed", "EmailConfirmtionKey", "EncriptionKey", "FirstName", "ImageUrl", "LastModifyDate", "LastName", "MiddelName", "Password", "PhoneConfirmationCode", "PhoneConfirmed", "PhoneNumber", "Region", "UserName", "UserType" },
                values: new object[,]
                {
                    { new Guid("d4c7be15-c9b6-4d83-8516-aff52c94f963"), null, null, null, null, new DateTime(2024, 9, 27, 21, 6, 0, 252, DateTimeKind.Unspecified).AddTicks(9751), null, null, false, null, "S4qu2kt4eTBicZKp5inKySLYhcvTZUfu", "Ali", "", new DateTime(2024, 9, 27, 21, 6, 0, 252, DateTimeKind.Unspecified).AddTicks(9751), "Fath", null, "JA848ibn1deWJVFuHcsLXx7Bxo4NBelM0gF2tu9nwGI=", null, true, "01288826193", null, "Ali", 1 },
                    { new Guid("de36f342-fe3c-46c3-bdfc-bb3fcf2ec7e4"), null, null, null, null, new DateTime(2024, 9, 27, 21, 6, 0, 252, DateTimeKind.Unspecified).AddTicks(9751), null, null, false, null, "UMvC6sSFY2ujmzh6A1aaK3QHEBS00QUa", "Ali", "", new DateTime(2024, 9, 27, 21, 6, 0, 252, DateTimeKind.Unspecified).AddTicks(9751), "Fath", null, "gNL9jSV6CPO827CIcQlRcgFHuqH5gmfAfn95jWLI9ec=", null, true, "01210199123", null, "AliFath", 0 }
                });

            migrationBuilder.InsertData(
                table: "Schools",
                columns: new[] { "Id", "About", "Active", "AvailableCoins", "CoinTypeId", "CreateDate", "ImageUrl", "LastModifyDate", "Name", "SchoolAdminId", "SchoolTypeId", "Vesion" },
                values: new object[] { new Guid("3338e17d-280f-467f-938a-5629415b6e52"), "", true, 100, new Guid("aa619624-c134-4e20-867b-e635a5a3b609"), new DateTime(2024, 9, 27, 21, 6, 0, 252, DateTimeKind.Unspecified).AddTicks(9751), "", new DateTime(2024, 9, 27, 21, 6, 0, 252, DateTimeKind.Unspecified).AddTicks(9751), "Demo School", new Guid("d4c7be15-c9b6-4d83-8516-aff52c94f963"), new Guid("04d490ad-0994-404e-9d8b-8ecf504811f3"), "" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("0225dbd5-9675-438c-87f2-63fb6921841c"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("072c7a82-b62d-45d8-a3ed-48a70319abf3"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("122d8e7a-f5a7-48ea-b2a6-f86964a2c3e7"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("219007ea-620e-4d96-8292-2d015ef68db1"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("68713df9-ed1b-449c-8060-d919a2592b02"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("69a0778b-a7a8-4e47-b8fd-d061428dbb95"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("71b6f158-82f2-4a48-9950-56dbb00eec4e"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("76befb0e-5ef7-4fe3-8e06-02d50c17c38b"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("8a48610d-c9ff-4fce-95eb-3a4d8d633a9c"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("fdebaade-4d2c-447b-ba52-11e3e2a622f4"), new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("056ed8bb-3ffc-459f-831e-c8a462db313b"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("062ff041-c775-4b30-8a49-202db7d9dd28"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("09e73f16-bb7a-403b-a8f9-fdc1c3228b0d"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("0d9bfad2-0762-47ba-8046-3aa92b2d6222"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("11f0aa92-e7af-4f94-bd50-96f389deee2a"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("16983650-564e-4331-97d2-c1b5d67fef40"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("21040e37-d949-42ea-9a77-96aed0289209"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("26de4395-48fb-4684-b88d-5c9e6081f9ac"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("2f1290b1-f439-4810-ba25-5c3e9cc56efe"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("30aa5979-36f6-4a63-98d2-a06f96178176"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("32d821bb-0c50-4721-9034-097019632c05"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("3b49fc12-d345-41c4-b5d0-97a388aaafa3"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("494d9c56-558a-427c-9854-878520fcdec8"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("49869e47-2209-4fd8-aa08-2e99ba211ff1"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("5c8d7610-bfaa-4deb-bd84-e54ed4351611"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("65b57f13-161f-43b2-9e8d-daafb50fbbbb"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("6de52188-1c80-46e3-a436-ff36469e2976"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("6ed86695-9ea7-4820-bf89-0de16dfd6ea9"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("75162229-2536-4232-b081-feabe20c318d"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("77127635-8fe8-4451-ac9c-59c38ee02d4d"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("7a382b52-95f3-442f-abff-9c1c5c9607cc"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("7b4478c8-2526-493a-a34d-9fffa5786f85"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("84956c26-e46f-432a-9b44-c8fb17e95552"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("861ad498-a214-4b7d-bde7-815abf63a587"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("869a6521-2730-4f0b-973b-60fb8093c769"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("93fd0aa6-852f-4afe-8cf4-9cdeb365d498"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("98d37ade-fdc0-4a0b-8e00-402a708c9ddd"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("99fdfba4-ef5b-44d1-b9bf-b4faf68a7b60"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("9dd22e15-9701-492b-ae20-985b8927f3bf"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("a1134102-7021-4770-8808-fdc376190691"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("a4a93ceb-21f9-4400-9444-0d94cf895beb"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("a992e74b-4ce4-4d18-bcc5-c079d116d1f5"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("ad53e402-c2ab-4941-861b-a19c82dca0cc"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("b65324a3-35b9-4579-8c85-c9938391dfa6"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("b6858fe8-dcc3-4c0f-b1a9-6a8ab7331924"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("b6d6c7e2-e2de-4b95-9789-a61c69f27765"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("b7bcf498-f7e9-48b4-9e99-49f6d0a9a956"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("b7f4d3f3-b0b9-4e2f-8884-e2d61e0b6f5e"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("baa954fd-8ba9-4529-834d-e640057998af"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("be86c901-37f9-4f63-b7b3-03653e75fea5"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("c1856505-7c6b-4932-8034-cf4fbc0efb67"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("c50d6574-648b-4f6e-8ed5-d27f8b5a34f1"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("cb37bbd0-40ad-4ca8-ae19-1ed87fea0b97"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("cc4a8f98-fbb7-49dd-8b5f-cc6d02bccab0"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("d512c1b3-dde0-4646-98cc-2be5c56c7150"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("d9b2c724-c5cc-497a-a11b-3e48b7dbfd97"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("d9bdcf84-d193-4775-8f9b-bacb1bd22915"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("da3e8df0-4d2b-412a-82d5-3b8bcf62d827"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("dd0f56ad-6039-4a8b-9d26-ebd5abe87e7d"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("de53cf00-e53d-4be0-8f67-71f8c7248df4"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("e09a3a9f-0ff1-46b7-8c80-b7f07ccb4542"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("e50a5830-e741-4260-820c-19c2cab1b419"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("e5b8454f-a193-4d63-b791-3a02aa9d71d3"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("e831e41d-1090-4ad0-bbaf-e4ca823ecd9d"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("ebd73c8f-2ef9-4a67-99e6-bafe71c9e83c"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("ecd2236f-6fa1-43ec-92e9-aa36ae2358b4"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("ee6325a4-4bc4-4aa7-9944-f82c3b7a305a"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("eec01fe0-4399-44b0-b8e7-afd417a6a93c"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("f19ccd22-35ac-4778-8a7f-fb0e56822385"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("fba69a66-4e31-44f2-93ac-9602705a7f98"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("fdebaade-4d2c-447b-ba52-11e3e2a622f4"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("fe286e0a-193b-4f12-ae88-df5808fb2eb7"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "Id",
                keyValue: new Guid("3338e17d-280f-467f-938a-5629415b6e52"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("de36f342-fe3c-46c3-bdfc-bb3fcf2ec7e4"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("d4c7be15-c9b6-4d83-8516-aff52c94f963"));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1"),
                column: "Name",
                value: "Application Owner");

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Active", "AdminRole", "CreateDate", "Description", "LastModifyDate", "Name", "SchoolRole" },
                values: new object[] { new Guid("6e473a7a-083c-405b-8f0d-04dd85b94edb"), true, true, null, "This Role with fuly access to all Application featuers", null, "Application Manager", false });
        }
    }
}
