using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MoveSchoolRelationFromUserPermissionSchoolClassRoomToUserPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_userPermissionSchoolClassRooms",
                table: "userPermissionSchoolClassRooms");

            migrationBuilder.DropPrimaryKey(
                name: "PK_userPermissions",
                table: "userPermissions");

            migrationBuilder.DropColumn(
                name: "SchoolId",
                table: "userPermissionSchoolClassRooms");

            migrationBuilder.AddColumn<Guid>(
                name: "SchoolId",
                table: "userPermissions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_userPermissionSchoolClassRooms",
                table: "userPermissionSchoolClassRooms",
                columns: new[] { "UserId", "PermissionId", "ClassRoomId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_userPermissions",
                table: "userPermissions",
                columns: new[] { "UserId", "PermissionId", "SchoolId" });

            migrationBuilder.CreateIndex(
                name: "IX_userPermissions_SchoolId",
                table: "userPermissions",
                column: "SchoolId");

            migrationBuilder.AddForeignKey(
                name: "FK_userPermissions_Schools_SchoolId",
                table: "userPermissions",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_userPermissions_Schools_SchoolId",
                table: "userPermissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_userPermissionSchoolClassRooms",
                table: "userPermissionSchoolClassRooms");

            migrationBuilder.DropPrimaryKey(
                name: "PK_userPermissions",
                table: "userPermissions");

            migrationBuilder.DropIndex(
                name: "IX_userPermissions_SchoolId",
                table: "userPermissions");

            migrationBuilder.DropColumn(
                name: "SchoolId",
                table: "userPermissions");

            migrationBuilder.AddColumn<Guid>(
                name: "SchoolId",
                table: "userPermissionSchoolClassRooms",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_userPermissionSchoolClassRooms",
                table: "userPermissionSchoolClassRooms",
                columns: new[] { "UserId", "PermissionId", "SchoolId", "ClassRoomId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_userPermissions",
                table: "userPermissions",
                columns: new[] { "UserId", "PermissionId" });
        }
    }
}
