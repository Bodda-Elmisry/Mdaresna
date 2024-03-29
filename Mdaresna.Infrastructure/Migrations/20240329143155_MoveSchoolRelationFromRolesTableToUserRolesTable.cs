using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MoveSchoolRelationFromRolesTableToUserRolesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Schools_SchoolId",
                table: "Roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_Roles_SchoolId",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "SchoolId",
                table: "Roles");

            migrationBuilder.AddColumn<Guid>(
                name: "SchoolId",
                table: "UserRoles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId", "SchoolId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_SchoolId",
                table: "UserRoles",
                column: "SchoolId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Schools_SchoolId",
                table: "UserRoles",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Schools_SchoolId",
                table: "UserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_SchoolId",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "SchoolId",
                table: "UserRoles");

            migrationBuilder.AddColumn<Guid>(
                name: "SchoolId",
                table: "Roles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.CreateIndex(
                name: "IX_Roles_SchoolId",
                table: "Roles",
                column: "SchoolId");

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Schools_SchoolId",
                table: "Roles",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "Id");
        }
    }
}
