using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolContactActionType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ActionType",
                table: "SchoolContactTypes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "SchoolContactTypes",
                keyColumn: "Id",
                keyValue: new Guid("04415888-fe5c-4c91-a8aa-b6a8d1383c08"),
                column: "ActionType",
                value: 2);

            migrationBuilder.UpdateData(
                table: "SchoolContactTypes",
                keyColumn: "Id",
                keyValue: new Guid("3851e877-81ec-4e74-a9ee-ab29265e873f"),
                column: "ActionType",
                value: 4);

            migrationBuilder.UpdateData(
                table: "SchoolContactTypes",
                keyColumn: "Id",
                keyValue: new Guid("5160b1a7-b5ff-4807-a3e0-94fd99579407"),
                column: "ActionType",
                value: 3);

            migrationBuilder.UpdateData(
                table: "SchoolContactTypes",
                keyColumn: "Id",
                keyValue: new Guid("7b962cc1-db7b-489f-b75e-a478fb932e00"),
                column: "ActionType",
                value: 2);

            migrationBuilder.UpdateData(
                table: "SchoolContactTypes",
                keyColumn: "Id",
                keyValue: new Guid("a3fdcfa4-0c57-416b-91a9-51e8601e7d0c"),
                column: "ActionType",
                value: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActionType",
                table: "SchoolContactTypes");
        }
    }
}
