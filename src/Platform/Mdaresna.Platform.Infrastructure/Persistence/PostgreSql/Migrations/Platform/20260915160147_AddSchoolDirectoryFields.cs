using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Platform.Infrastructure.Persistence.PostgreSql.Migrations.Platform
{
    /// <inheritdoc />
    public partial class AddSchoolDirectoryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ActivatedAtUtc",
                schema: "registry",
                table: "schools",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                schema: "registry",
                table: "schools",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UnitTypeId",
                schema: "registry",
                table: "schools",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_schools_UnitTypeId",
                schema: "registry",
                table: "schools",
                column: "UnitTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_schools_unit_types_UnitTypeId",
                schema: "registry",
                table: "schools",
                column: "UnitTypeId",
                principalSchema: "billing",
                principalTable: "unit_types",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_schools_unit_types_UnitTypeId",
                schema: "registry",
                table: "schools");

            migrationBuilder.DropIndex(
                name: "IX_schools_UnitTypeId",
                schema: "registry",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "ActivatedAtUtc",
                schema: "registry",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "Address",
                schema: "registry",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "UnitTypeId",
                schema: "registry",
                table: "schools");
        }
    }
}
