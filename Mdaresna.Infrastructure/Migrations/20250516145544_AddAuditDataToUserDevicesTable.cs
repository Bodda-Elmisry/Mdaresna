using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditDataToUserDevicesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "UserDevices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "UserDevices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                table: "UserDevices",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "UserDevices");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "UserDevices");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                table: "UserDevices");
        }
    }
}
