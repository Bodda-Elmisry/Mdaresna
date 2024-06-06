using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeCoinTypeInSchoolsTableToAllowNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schools_CoinsTypes_CoinTypeId",
                table: "Schools");

            migrationBuilder.AlterColumn<Guid>(
                name: "CoinTypeId",
                table: "Schools",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("42f73d52-5e31-485a-8f5b-6f53670447ca"),
                columns: new[] { "CreateDate", "LastModifyDate" },
                values: new object[] { new DateTime(2024, 6, 4, 23, 0, 54, 602, DateTimeKind.Local), new DateTime(2024, 6, 4, 23, 0, 54, 602, DateTimeKind.Local) });

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("7ff35951-0ad2-46c7-83a5-f4487365ec1c"),
                columns: new[] { "CreateDate", "LastModifyDate" },
                values: new object[] { new DateTime(2024, 6, 4, 23, 0, 54, 602, DateTimeKind.Local), new DateTime(2024, 6, 4, 23, 0, 54, 602, DateTimeKind.Local) });

            migrationBuilder.AddForeignKey(
                name: "FK_Schools_CoinsTypes_CoinTypeId",
                table: "Schools",
                column: "CoinTypeId",
                principalTable: "CoinsTypes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schools_CoinsTypes_CoinTypeId",
                table: "Schools");

            migrationBuilder.AlterColumn<Guid>(
                name: "CoinTypeId",
                table: "Schools",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("42f73d52-5e31-485a-8f5b-6f53670447ca"),
                columns: new[] { "CreateDate", "LastModifyDate" },
                values: new object[] { new DateTime(2024, 6, 4, 23, 0, 54, 602, DateTimeKind.Local).AddTicks(9882), new DateTime(2024, 6, 4, 23, 0, 54, 602, DateTimeKind.Local).AddTicks(9884) });

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("7ff35951-0ad2-46c7-83a5-f4487365ec1c"),
                columns: new[] { "CreateDate", "LastModifyDate" },
                values: new object[] { new DateTime(2024, 6, 4, 23, 0, 54, 602, DateTimeKind.Local).AddTicks(9807), new DateTime(2024, 6, 4, 23, 0, 54, 602, DateTimeKind.Local).AddTicks(9877) });

            migrationBuilder.AddForeignKey(
                name: "FK_Schools_CoinsTypes_CoinTypeId",
                table: "Schools",
                column: "CoinTypeId",
                principalTable: "CoinsTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
