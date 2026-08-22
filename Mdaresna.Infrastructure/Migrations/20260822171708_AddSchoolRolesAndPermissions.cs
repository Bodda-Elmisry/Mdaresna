using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolRolesAndPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SchoolId",
                table: "Roles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AvailableForSchoolCustomRoles",
                table: "Permissions",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("006edbb6-657d-41cf-9e01-8ab9cf4cea69"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("0225dbd5-9675-438c-87f2-63fb6921841c"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("056ed8bb-3ffc-459f-831e-c8a462db313b"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("061ac24b-3828-4360-833f-ef5865712e39"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("062ff041-c775-4b30-8a49-202db7d9dd28"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("072c7a82-b62d-45d8-a3ed-48a70319abf3"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("09756cb3-4363-4763-812a-13f1f8a3b693"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("09e73f16-bb7a-403b-a8f9-fdc1c3228b0d"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("0a0ab8a7-1d51-4b03-b10a-647a8f90ec24"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("0c7c9f0e-8f3c-4f44-9d83-0c2e5b1c61f1"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("0d9bfad2-0762-47ba-8046-3aa92b2d6222"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("11f0aa92-e7af-4f94-bd50-96f389deee2a"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("122d8e7a-f5a7-48ea-b2a6-f86964a2c3e7"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("132ec233-d6ef-4187-a780-e53c85d1babb"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("16983650-564e-4331-97d2-c1b5d67fef40"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("16b49575-3825-4489-9822-a64615fe8898"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("19ea90dc-1ed5-4445-a238-9c95e2f37842"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("1dfbdb17-ba97-4539-b887-e81fc0e72b47"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("21040e37-d949-42ea-9a77-96aed0289209"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("219007ea-620e-4d96-8292-2d015ef68db1"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("21e7443f-566f-452e-a246-4e65260b16f4"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("23a9e98e-ba77-48bb-8f92-73fae4df3245"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("26b6aad5-3434-4538-867e-165d70800fb9"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("26de4395-48fb-4684-b88d-5c9e6081f9ac"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("27daf2ff-556d-4515-a41f-dead5699f5ab"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("2989c158-ef37-40f8-bdf8-637c874f2f1e"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("2b656fc9-b90a-4b22-b882-672495224220"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("2e6d4d17-1e34-4f7d-a8d8-1e4b1c737bb4"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("2f1290b1-f439-4810-ba25-5c3e9cc56efe"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("2f14a9b5-3270-4391-ab66-0b09a59c460d"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("30aa5979-36f6-4a63-98d2-a06f96178176"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("32d821bb-0c50-4721-9034-097019632c05"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("37fc9ece-b339-446e-beba-a81b71302266"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("3992521c-f222-4323-8969-94f23987f157"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("3b49fc12-d345-41c4-b5d0-97a388aaafa3"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("3f72084d-f56b-46eb-ab36-a0cd5956f55b"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("44c76680-9b72-48f9-bf3d-f113c4447331"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("4838711a-4139-465e-a34f-a4b6756ae475"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("494d9c56-558a-427c-9854-878520fcdec8"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("49869e47-2209-4fd8-aa08-2e99ba211ff1"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("4c1e2d42-8f6c-4b8a-9a80-6a8f6d034d5a"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("512d2ebf-dd4a-482b-8753-1252fe196511"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("55f6b349-57d1-40d0-bb56-64c1592c2268"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("56d9a5b6-28a6-4e8d-ade6-54ad37c846bd"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("5b4c93d4-22e3-4d89-bdb8-6b6ec2f7e840"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("5c8d7610-bfaa-4deb-bd84-e54ed4351611"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("5e48c040-34e8-4905-9b15-3f4069e39840"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("5ecbb154-8466-4ea1-8b66-8aa1e7d7853a"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("5ed6b95d-cd84-4b2c-beb8-88686933ea78"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("5f3ff85f-d4bb-46be-b0e6-942c10a87b4a"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("65b57f13-161f-43b2-9e8d-daafb50fbbbb"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("666f4016-4375-4ca0-ba48-6c1cab50f91a"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("68713df9-ed1b-449c-8060-d919a2592b02"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("687a31c3-6d21-4f8b-9860-379b2a563758"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("69a0778b-a7a8-4e47-b8fd-d061428dbb95"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("6de52188-1c80-46e3-a436-ff36469e2976"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("6ed86695-9ea7-4820-bf89-0de16dfd6ea9"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("71b6f158-82f2-4a48-9950-56dbb00eec4e"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("75162229-2536-4232-b081-feabe20c318d"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("76befb0e-5ef7-4fe3-8e06-02d50c17c38b"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("77127635-8fe8-4451-ac9c-59c38ee02d4d"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("7a382b52-95f3-442f-abff-9c1c5c9607cc"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("7b4478c8-2526-493a-a34d-9fffa5786f85"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("84956c26-e46f-432a-9b44-c8fb17e95552"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("861ad498-a214-4b7d-bde7-815abf63a587"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("869a6521-2730-4f0b-973b-60fb8093c769"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("889bc51f-0624-4911-988d-4a86f1a4f011"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("893e8a43-0da7-4149-abdb-e2469239896f"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("8a01a316-9151-4bb7-8b0e-a87e5ee7e367"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("8a48610d-c9ff-4fce-95eb-3a4d8d633a9c"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("8ad5e47c-5ec4-49c7-a0ab-0d37e576961f"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("8e296936-5161-4b61-885c-b520fe350c9a"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("913eeed8-4d9d-4778-a84d-3178b36fedbb"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("9301fc37-ae75-4ef6-b6fa-a656452e5a2e"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("93fd0aa6-852f-4afe-8cf4-9cdeb365d498"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("97aad235-16fa-496b-88d2-adceefbd8d5c"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("98d37ade-fdc0-4a0b-8e00-402a708c9ddd"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("99542671-e575-43f7-9c67-5290d9cf4578"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("99fdfba4-ef5b-44d1-b9bf-b4faf68a7b60"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("9c8526e9-9119-43d8-a434-4c4828c8a5d9"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("9d7ec7f9-0341-48a6-8db5-7486bca97497"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("9dd22e15-9701-492b-ae20-985b8927f3bf"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a1134102-7021-4770-8808-fdc376190691"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a1bd5ef1-6a55-4e1e-8bb8-da39344e4412"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a44f8b6e-7b16-4497-9c4b-8e55eaf4e7d4"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a4a93ceb-21f9-4400-9444-0d94cf895beb"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a8704499-413d-4ff4-a3a2-122b684a0e17"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a992e74b-4ce4-4d18-bcc5-c079d116d1f5"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ac0c5e27-8f01-4b94-bef0-88feaae2043a"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ad53e402-c2ab-4941-861b-a19c82dca0cc"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("af2eedf8-ee6d-464b-8551-743f6ef3d3b5"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b15e32eb-092e-437d-9d4e-b9ed583c23b0"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b357214a-296e-4999-be38-2af4259d3096"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b65324a3-35b9-4579-8c85-c9938391dfa6"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b6858fe8-dcc3-4c0f-b1a9-6a8ab7331924"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b6d6c7e2-e2de-4b95-9789-a61c69f27765"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b7bcf498-f7e9-48b4-9e99-49f6d0a9a956"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b7f4d3f3-b0b9-4e2f-8884-e2d61e0b6f5e"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b96d60ad-e4db-4ce9-8ea8-c22d2ce8c544"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("baa954fd-8ba9-4529-834d-e640057998af"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("be86c901-37f9-4f63-b7b3-03653e75fea5"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("c0fe2438-b76a-4bcb-9cde-d628b5c3deb6"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("c1856505-7c6b-4932-8034-cf4fbc0efb67"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("c1f5a23e-3b9c-4e57-8ad8-1bbf2f7a2c4d"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("c3da80d1-9288-4ebe-96e7-7210cbeeb1ca"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("c50d6574-648b-4f6e-8ed5-d27f8b5a34f1"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("cb37bbd0-40ad-4ca8-ae19-1ed87fea0b97"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("cbcff607-eac1-45e8-81e7-4626a165a1b9"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("cc4a8f98-fbb7-49dd-8b5f-cc6d02bccab0"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("cdc3c1c4-4598-44f9-904f-b5d19e31f328"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("cf67bebe-a01e-46e0-9b8e-1edc0cee2087"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d0b2932f-717a-4ad7-83a8-c11f1806237d"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d330df2e-9b66-46a0-a64d-88b1d4f9519d"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d512c1b3-dde0-4646-98cc-2be5c56c7150"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d65974e4-239a-4683-8b07-5110270e04f6"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d9b2c724-c5cc-497a-a11b-3e48b7dbfd97"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d9bb6050-186d-4052-a3dc-6d33a84424ac"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d9bdcf84-d193-4775-8f9b-bacb1bd22915"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("daf8b889-ee84-45e2-9c59-8135cd5551a7"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("dd0f56ad-6039-4a8b-9d26-ebd5abe87e7d"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("dda9e1f1-00a0-4a68-84e7-8e24b9a6c7f1"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("de402af3-0c36-4204-aa5b-df56f8033580"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("de53cf00-e53d-4be0-8f67-71f8c7248df4"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("e09a3a9f-0ff1-46b7-8c80-b7f07ccb4542"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("e291a793-6efa-40e1-878d-7c11095a6c65"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("e4c90ea7-7f44-4c81-82b7-b1ddd979b9cd"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("e50a5830-e741-4260-820c-19c2cab1b419"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("e593cd01-0ff3-4ae1-9df6-61e9d7c23f5d"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("e5b8454f-a193-4d63-b791-3a02aa9d71d3"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("e831e41d-1090-4ad0-bbaf-e4ca823ecd9d"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("e9c02932-b613-45eb-9e71-7cb6204745d9"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ea7e7a5d-64c0-4ae6-b8b1-70a7d7e4f66c"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ebd73c8f-2ef9-4a67-99e6-bafe71c9e83c"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ebde1c53-4840-484f-ac23-df838e6282dc"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ec2c9457-2184-4b45-b271-a90a461a816e"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ecd2236f-6fa1-43ec-92e9-aa36ae2358b4"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ee6325a4-4bc4-4aa7-9944-f82c3b7a305a"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("eec01fe0-4399-44b0-b8e7-afd417a6a93c"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("f19ccd22-35ac-4778-8a7f-fb0e56822385"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("f4c86ead-e913-4c5e-b1a5-3e17a647c216"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("f6821a1f-cb86-445d-b7aa-b7224bd11b47"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("f76d6cd8-4a98-4eba-9d09-9204009d7839"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("fb4f053d-67fa-4ff2-b85f-a4bf8f385bce"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("fba69a66-4e31-44f2-93ac-9602705a7f98"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("fbadcd2d-c9c8-4164-bad1-667a586b54cc"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("fdebaade-4d2c-447b-ba52-11e3e2a622f4"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("fe286e0a-193b-4f12-ae88-df5808fb2eb7"),
                column: "AvailableForSchoolCustomRoles",
                value: true);

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "AppPermission", "AvailableForSchoolCustomRoles", "CreateDate", "Description", "Description_AR", "Key", "LastModifyDate", "Name", "Name_AR", "SchoolPermission" },
                values: new object[,]
                {
                    { new Guid("a1b2c3d4-e5f6-4789-a012-3456789abc01"), false, true, null, "View school roles list", "عرض قائمة أدوار المدرسة", "ViewSchoolRoles", null, "View School Roles", "عرض أدوار المدرسة", true },
                    { new Guid("b2c3d4e5-f6a7-4890-b123-456789abc012"), false, true, null, "Create custom school role", "إنشاء دور مخصص للمدرسة", "CreateSchoolRole", null, "Create School Role", "إنشاء دور مخصص للمدرسة", true },
                    { new Guid("c3d4e5f6-a7b8-4901-c234-56789abc0123"), false, true, null, "Edit custom school role", "تعديل دور مخصص للمدرسة", "EditSchoolRole", null, "Edit School Role", "تعديل دور مخصص للمدرسة", true },
                    { new Guid("d4e5f6a7-b8c9-4012-d345-6789abc01234"), false, true, null, "Delete custom school role", "حذف دور مخصص للمدرسة", "DeleteSchoolRole", null, "Delete School Role", "حذف دور مخصص للمدرسة", true }
                });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("10620c5f-37fe-4d18-996f-915ece8893f1"),
                column: "SchoolId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("228ae7f5-c704-4660-aeb0-0e1f43112ae1"),
                column: "SchoolId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"),
                column: "SchoolId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("92d00b28-9d25-4bd2-a587-6c22a3a07a92"),
                column: "SchoolId",
                value: null);

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId", "CreateDate", "LastModifyDate" },
                values: new object[,]
                {
                    { new Guid("a1b2c3d4-e5f6-4789-a012-3456789abc01"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("b2c3d4e5-f6a7-4890-b123-456789abc012"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("c3d4e5f6-a7b8-4901-c234-56789abc0123"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null },
                    { new Guid("d4e5f6a7-b8c9-4012-d345-6789abc01234"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73"), null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Roles_SchoolId",
                table: "Roles",
                column: "SchoolId");

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Schools_SchoolId",
                table: "Roles",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Schools_SchoolId",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Roles_SchoolId",
                table: "Roles");

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("a1b2c3d4-e5f6-4789-a012-3456789abc01"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("b2c3d4e5-f6a7-4890-b123-456789abc012"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("c3d4e5f6-a7b8-4901-c234-56789abc0123"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("d4e5f6a7-b8c9-4012-d345-6789abc01234"), new Guid("4b8a99fe-b759-4c18-9500-8052c3d7ac73") });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-4789-a012-3456789abc01"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b2c3d4e5-f6a7-4890-b123-456789abc012"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("c3d4e5f6-a7b8-4901-c234-56789abc0123"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d4e5f6a7-b8c9-4012-d345-6789abc01234"));

            migrationBuilder.DropColumn(
                name: "SchoolId",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "AvailableForSchoolCustomRoles",
                table: "Permissions");
        }
    }
}
