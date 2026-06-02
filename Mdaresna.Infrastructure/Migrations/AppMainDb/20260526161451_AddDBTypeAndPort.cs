using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations.AppMainDb
{
    /// <inheritdoc />
    public partial class AddDBTypeAndPort : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DBPort",
                table: "Services",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DBType",
                table: "Services",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DBPort",
                table: "SchoolServices",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DBType",
                table: "SchoolServices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DBPort",
                table: "Schools",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DBType",
                table: "Schools",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "Services",
                columns: new[] { "Id", "CreateDate", "DBPort", "DBSource", "DBType", "DBUser", "Deleted", "IsActive", "LastModifyDate", "Name" },
                values: new object[] { new Guid("8488e63b-fd78-43bf-800b-03412c372db5"), new DateTime(2026, 5, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "5432", "localhost", 2, "postgres", false, true, null, "ReportingService" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("8488e63b-fd78-43bf-800b-03412c372db5"));

            migrationBuilder.DropColumn(
                name: "DBPort",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "DBType",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "DBPort",
                table: "SchoolServices");

            migrationBuilder.DropColumn(
                name: "DBType",
                table: "SchoolServices");

            migrationBuilder.DropColumn(
                name: "DBPort",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "DBType",
                table: "Schools");
        }
    }
}
