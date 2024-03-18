using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolRelationToRolesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Schools_SchoolId",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Roles_SchoolId",
                table: "Roles");
        }
    }
}
