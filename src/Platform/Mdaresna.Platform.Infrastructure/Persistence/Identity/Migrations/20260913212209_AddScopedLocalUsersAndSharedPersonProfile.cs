using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Platform.Infrastructure.Persistence.Identity.Migrations
{
    /// <inheritdoc />
    public partial class AddScopedLocalUsersAndSharedPersonProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "DateOfBirth",
                schema: "identity",
                table: "accounts",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GenderCode",
                schema: "identity",
                table: "accounts",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                schema: "identity",
                table: "accounts");

            migrationBuilder.DropColumn(
                name: "GenderCode",
                schema: "identity",
                table: "accounts");
        }
    }
}
